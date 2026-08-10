using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace CricketApp
{
    public partial class Admin : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTeams();
                int liveMatchId = GetLiveMatchId();
                RefreshLogs(liveMatchId);
            }
        }

        // --- NEW: Load Teams into Dropdowns ---
        private void LoadTeams()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT TeamId, TeamName FROM Teams", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlBattingTeam.DataSource = dt;
                ddlBattingTeam.DataBind();

                ddlBowlingTeam.DataSource = dt;
                ddlBowlingTeam.DataBind();

                ddlWinner.DataSource = dt;
                ddlWinner.DataBind();

                ddlTossWinner.DataSource = dt;
                ddlTossWinner.DataBind();
            }
        }

        // --- NEW: Start a Match ---
        protected void btnStartMatch_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. Turn off any currently live matches
                SqlCommand cmdOff = new SqlCommand("UPDATE Matches SET IsLive = 0", conn);
                cmdOff.ExecuteNonQuery();

                // 2. Create the new match
                string insertMatch = @"
    INSERT INTO Matches (Team1Id, Team2Id, TossWinnerId, IsLive) 
    OUTPUT INSERTED.MatchId 
    VALUES (@Team1, @Team2, @TossWinner, 1)";
                SqlCommand cmdNew = new SqlCommand(insertMatch, conn);
                cmdNew.Parameters.AddWithValue("@Team1", ddlBattingTeam.SelectedValue);
                cmdNew.Parameters.AddWithValue("@Team2", ddlBowlingTeam.SelectedValue);
                cmdNew.Parameters.AddWithValue("@TossWinner", ddlTossWinner.SelectedValue);

                // Get the new Match ID
                int newMatchId = (int)cmdNew.ExecuteScalar();

                // 3. Create the starting score board (0 runs, 0 wickets)
                string insertState = @"
    INSERT INTO MatchState (MatchId, BattingTeamId, TeamScore, Wickets, Overs, Balls, LastUpdated, Innings, TargetScore) 
    VALUES (@MatchId, @BattingId, 0, 0, 0, 0, GETDATE(), 1, 0)";
                SqlCommand cmdState = new SqlCommand(insertState, conn);
                cmdState.Parameters.AddWithValue("@MatchId", newMatchId);
                cmdState.Parameters.AddWithValue("@BattingId", ddlBattingTeam.SelectedValue);
                cmdState.ExecuteNonQuery();

                lblMatchMessage.Text = "New Match Started Successfully! Score reset to 0/0.";

                // At the very bottom of btnStartMatch_Click, before closing the bracket, add:
                WriteLog(newMatchId, $"MATCH STARTED: {ddlBattingTeam.SelectedItem.Text} vs {ddlBowlingTeam.SelectedItem.Text}.");
                RefreshLogs(newMatchId);
            }
        }

        // --- NEW: End Match & Save Winner ---
        protected void btnEndMatch_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                // Update the current live match with the winner, and set it to not live
                SqlCommand cmdEnd = new SqlCommand("UPDATE Matches SET WinnerTeamId = @WinnerId, IsLive = 0 WHERE IsLive = 1", conn);
                cmdEnd.Parameters.AddWithValue("@WinnerId", ddlWinner.SelectedValue);
                cmdEnd.ExecuteNonQuery();

                lblMatchMessage.Text = "Match Ended and Winner Recorded!";

                // At the very bottom of btnEndMatch_Click, before closing the bracket, add:
                int matchId = GetLiveMatchId();
                WriteLog(matchId, $"MATCH ENDED: The winner is {ddlWinner.SelectedItem.Text}.");
                RefreshLogs(matchId);
            }
        }


        // --- BUTTON CLICK EVENTS (Keep these exactly the same) ---
        protected void btnDot_Click(object sender, EventArgs e) { ProcessBall(0, 0, true); }
        protected void btn1Run_Click(object sender, EventArgs e) { ProcessBall(1, 0, true); }
        protected void btn2Runs_Click(object sender, EventArgs e) { ProcessBall(2, 0, true); }

        // ADDED EVENT HANDLERS FOR 3 AND 5 RUNS
        protected void btn3Runs_Click(object sender, EventArgs e) { ProcessBall(3, 0, true); }
        protected void btn5Runs_Click(object sender, EventArgs e) { ProcessBall(5, 0, true); }

        protected void btnFour_Click(object sender, EventArgs e) { ProcessBall(4, 0, true); }
        protected void btnSix_Click(object sender, EventArgs e) { ProcessBall(6, 0, true); }
        protected void btnWicket_Click(object sender, EventArgs e) { ProcessBall(0, 1, true); }
        protected void btnWide_Click(object sender, EventArgs e) { ProcessBall(1, 0, false); }
        protected void btnNoBall_Click(object sender, EventArgs e) { ProcessBall(1, 0, false); }

        // --- UPDATED LOGIC: Update the Live Match ---
        private void ProcessBall(int runsScored, int wicketsFallen, bool isLegalBall)
        {
            // Define the limits from the Python algorithm
            int maxOvers = 5;
            int maxWickets = 8;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. Get Current Score State & Team IDs
                string getScoreQuery = @"
    SELECT ms.TeamScore, ms.Wickets, ms.Overs, ms.Balls, ms.Innings, ms.TargetScore,
           m.Team1Id, m.Team2Id
    FROM MatchState ms
    INNER JOIN Matches m ON ms.MatchId = m.MatchId
    WHERE m.IsLive = 1";

                SqlCommand getCmd = new SqlCommand(getScoreQuery, conn);
                SqlDataReader reader = getCmd.ExecuteReader();

                int currentScore = 0, currentWickets = 0, currentOvers = 0, currentBalls = 0, currentInnings = 1, targetScore = 0;
                int battingTeamId = 0, bowlingTeamId = 0;

                if (reader.Read())
                {
                    currentScore = Convert.ToInt32(reader["TeamScore"]);
                    currentWickets = Convert.ToInt32(reader["Wickets"]);
                    currentOvers = Convert.ToInt32(reader["Overs"]);
                    currentBalls = Convert.ToInt32(reader["Balls"]);
                    currentInnings = Convert.ToInt32(reader["Innings"]);
                    targetScore = Convert.ToInt32(reader["TargetScore"]);

                    // Because teams are swapped at the 2nd innings, Team1Id is always the current batting team
                    battingTeamId = Convert.ToInt32(reader["Team1Id"]);
                    bowlingTeamId = Convert.ToInt32(reader["Team2Id"]);
                }
                else
                {
                    lblStatus.Text = "No live match started yet!";
                    return;
                }
                reader.Close();

                // --- BACKUP LOGIC ---
                int matchId = GetLiveMatchId();
                string backupQuery = @"
INSERT INTO MatchStateHistory (MatchId, TeamScore, Wickets, Overs, Balls, Innings, TargetScore) 
VALUES (@MatchId, @Score, @Wickets, @Overs, @Balls, @Innings, @Target)";

                SqlCommand backupCmd = new SqlCommand(backupQuery, conn);
                backupCmd.Parameters.AddWithValue("@MatchId", matchId);
                backupCmd.Parameters.AddWithValue("@Score", currentScore);
                backupCmd.Parameters.AddWithValue("@Wickets", currentWickets);
                backupCmd.Parameters.AddWithValue("@Overs", currentOvers);
                backupCmd.Parameters.AddWithValue("@Balls", currentBalls);
                backupCmd.Parameters.AddWithValue("@Innings", currentInnings);
                backupCmd.Parameters.AddWithValue("@Target", targetScore);
                backupCmd.ExecuteNonQuery();

                // --- ENFORCE LIMITS ---
                if (currentWickets >= maxWickets || currentOvers >= maxOvers || (currentInnings == 2 && currentScore >= targetScore))
                {
                    lblStatus.Text = "Cannot add runs. The innings/match is already over!";
                    return;
                }

                // 2. Apply the runs/wickets
                currentScore += runsScored;
                currentWickets += wicketsFallen;

                if (isLegalBall)
                {
                    currentBalls += 1;
                    if (currentBalls == 6)
                    {
                        currentBalls = 0;
                        currentOvers += 1;
                    }
                }

                // 3. Save to Database
                string updateQuery = @"
    UPDATE ms 
    SET ms.TeamScore = @Score, ms.Wickets = @Wickets, ms.Overs = @Overs, ms.Balls = @Balls, ms.LastUpdated = GETDATE()
    FROM MatchState ms INNER JOIN Matches m ON ms.MatchId = m.MatchId WHERE m.IsLive = 1";

                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@Score", currentScore);
                updateCmd.Parameters.AddWithValue("@Wickets", currentWickets);
                updateCmd.Parameters.AddWithValue("@Overs", currentOvers);
                updateCmd.Parameters.AddWithValue("@Balls", currentBalls);
                updateCmd.ExecuteNonQuery();

                // 4. Generate the Status, Winner Message & Check Auto-End Condition
                string statusMsg = $"Successfully updated! Score: {currentScore}/{currentWickets} in {currentOvers}.{currentBalls} overs.";

                bool isMatchOver = false;
                object finalWinnerId = DBNull.Value;

                if (currentInnings == 1)
                {
                    if (currentOvers >= maxOvers || currentWickets >= maxWickets)
                        statusMsg = $"INNINGS 1 OVER! The target is {currentScore + 1}. Please click 'Start 2nd Innings'.";
                }
                else if (currentInnings == 2)
                {
                    if (currentScore >= targetScore)
                    {
                        statusMsg = $"MATCH OVER! The batting team wins by successfully chasing the target of {targetScore}!";
                        isMatchOver = true;
                        finalWinnerId = battingTeamId; // Batting team successfully chased
                    }
                    else if (currentOvers >= maxOvers || currentWickets >= maxWickets)
                    {
                        if (currentScore == targetScore - 1)
                        {
                            statusMsg = $"MATCH OVER! It's a TIE!";
                            isMatchOver = true;
                            finalWinnerId = DBNull.Value; // Tied match leaves Winner ID as NULL
                        }
                        else
                        {
                            statusMsg = $"MATCH OVER! The bowling team wins by defending their target!";
                            isMatchOver = true;
                            finalWinnerId = bowlingTeamId; // Bowling team successfully defended
                        }
                    }
                    else
                    {
                        statusMsg += $" (Target: {targetScore})";
                    }
                }

                lblStatus.Text = statusMsg;

                // Create a descriptive message of what just happened
                string actionType = "Runs";
                if (wicketsFallen > 0) actionType = "Wicket";
                else if (!isLegalBall) actionType = "Extra (Wide/No Ball)";

                WriteLog(matchId, $"Action: {actionType} added. Board is now {currentScore}/{currentWickets} in {currentOvers}.{currentBalls} overs.");

                // 5. AUTOMATICALLY END THE MATCH
                if (isMatchOver)
                {
                    string endMatchQuery = "UPDATE Matches SET IsLive = 0, WinnerTeamId = @WinnerId WHERE MatchId = @MatchId";
                    SqlCommand endCmd = new SqlCommand(endMatchQuery, conn);
                    endCmd.Parameters.AddWithValue("@WinnerId", finalWinnerId);
                    endCmd.Parameters.AddWithValue("@MatchId", matchId);
                    endCmd.ExecuteNonQuery();

                    WriteLog(matchId, "SYSTEM: Match automatically ended and result recorded.");
                }

                RefreshLogs(matchId);
            }
        }

        protected void btnSecondInnings_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. Get current score to set as Target
                SqlCommand getCmd = new SqlCommand("SELECT TeamScore, ms.MatchId FROM MatchState ms INNER JOIN Matches m ON ms.MatchId = m.MatchId WHERE m.IsLive = 1", conn);
                SqlDataReader reader = getCmd.ExecuteReader();
                int currentScore = 0, matchId = 0;

                if (reader.Read())
                {
                    currentScore = Convert.ToInt32(reader["TeamScore"]);
                    matchId = Convert.ToInt32(reader["MatchId"]);
                }
                reader.Close();

                int targetScore = currentScore + 1;

                // 2. Swap teams in the Matches table so the Public UI updates automatically
                SqlCommand swapCmd = new SqlCommand(@"
            DECLARE @T1 INT, @T2 INT;
            SELECT @T1 = Team1Id, @T2 = Team2Id FROM Matches WHERE MatchId = @MatchId;
            UPDATE Matches SET Team1Id = @T2, Team2Id = @T1 WHERE MatchId = @MatchId;
        ", conn);
                swapCmd.Parameters.AddWithValue("@MatchId", matchId);
                swapCmd.ExecuteNonQuery();

                // 3. Reset Scoreboard, set Innings = 2, and UPDATE BattingTeamId
                SqlCommand updateStateCmd = new SqlCommand(@"
            -- Grab the newly swapped Team ID to set as the active batting team
            DECLARE @NewBattingId INT;
            SELECT @NewBattingId = Team1Id FROM Matches WHERE MatchId = @MatchId;

            UPDATE MatchState 
            SET TeamScore = 0, Wickets = 0, Overs = 0, Balls = 0, 
                Innings = 2, TargetScore = @Target, LastUpdated = GETDATE(),
                BattingTeamId = @NewBattingId
            WHERE MatchId = @MatchId", conn);

                updateStateCmd.Parameters.AddWithValue("@Target", targetScore);
                updateStateCmd.Parameters.AddWithValue("@MatchId", matchId);
                updateStateCmd.ExecuteNonQuery();

                lblMatchMessage.Text = $"2nd Innings Started! Teams swapped. Target score is {targetScore}.";
                lblStatus.Text = "";
            }
        }

        // --- LOGGING HELPER METHODS ---

        // 1. Gets the ID of the currently live match
        private int GetLiveMatchId()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("SELECT MatchId FROM Matches WHERE IsLive = 1", conn);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        // 2. Writes a new action to the database
        private void WriteLog(int matchId, string message)
        {
            if (matchId == 0) return; // No live match

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "INSERT INTO MatchLogs (MatchId, LogMessage, LogTime) VALUES (@MatchId, @Msg, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MatchId", matchId);
                cmd.Parameters.AddWithValue("@Msg", message);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 3. Fetches all logs for the current match and displays them in the TextBox
        private void RefreshLogs(int matchId)
        {
            txtActivityLog.Text = ""; // Clear the box first

            if (matchId == 0)
            {
                txtActivityLog.Text = "System: No live match currently active.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // CHANGED: ORDER BY LogTime DESC ensures the newest logs are pulled first
                string query = "SELECT LogTime, LogMessage FROM MatchLogs WHERE MatchId = @MatchId ORDER BY LogTime DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MatchId", matchId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string timeString = Convert.ToDateTime(reader["LogTime"]).ToString("hh:mm:ss tt");
                    string msg = reader["LogMessage"].ToString();

                    // Because the reader is handing us the newest records first, 
                    // appending them normally puts the newest action on line 1.
                    txtActivityLog.Text += $"[{timeString}] {msg}\r\n";
                }
            }
        }

        protected void btnUndo_Click(object sender, EventArgs e)
        {
            int matchId = GetLiveMatchId();
            if (matchId == 0)
            {
                lblUndoStatus.Text = "No live match to undo!";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. Get the most recent history record for the live match
                string getHistoryQuery = @"
            SELECT TOP 1 HistoryId, TeamScore, Wickets, Overs, Balls, Innings, TargetScore 
            FROM MatchStateHistory 
            WHERE MatchId = @MatchId 
            ORDER BY HistoryId DESC";

                SqlCommand getCmd = new SqlCommand(getHistoryQuery, conn);
                getCmd.Parameters.AddWithValue("@MatchId", matchId);
                SqlDataReader reader = getCmd.ExecuteReader();

                if (reader.Read())
                {
                    int historyId = Convert.ToInt32(reader["HistoryId"]);
                    int oldScore = Convert.ToInt32(reader["TeamScore"]);
                    int oldWickets = Convert.ToInt32(reader["Wickets"]);
                    int oldOvers = Convert.ToInt32(reader["Overs"]);
                    int oldBalls = Convert.ToInt32(reader["Balls"]);
                    int oldInnings = Convert.ToInt32(reader["Innings"]);
                    int oldTarget = Convert.ToInt32(reader["TargetScore"]);
                    reader.Close();

                    // 2. Overwrite the live MatchState with the old historical values
                    string restoreQuery = @"
                UPDATE MatchState 
                SET TeamScore = @Score, Wickets = @Wickets, Overs = @Overs, 
                    Balls = @Balls, Innings = @Innings, TargetScore = @Target, 
                    LastUpdated = GETDATE() 
                WHERE MatchId = @MatchId";

                    SqlCommand restoreCmd = new SqlCommand(restoreQuery, conn);
                    restoreCmd.Parameters.AddWithValue("@Score", oldScore);
                    restoreCmd.Parameters.AddWithValue("@Wickets", oldWickets);
                    restoreCmd.Parameters.AddWithValue("@Overs", oldOvers);
                    restoreCmd.Parameters.AddWithValue("@Balls", oldBalls);
                    restoreCmd.Parameters.AddWithValue("@Innings", oldInnings);
                    restoreCmd.Parameters.AddWithValue("@Target", oldTarget);
                    restoreCmd.Parameters.AddWithValue("@MatchId", matchId);
                    restoreCmd.ExecuteNonQuery();

                    // 3. Delete this history record so clicking "Undo" again steps back another ball
                    SqlCommand deleteCmd = new SqlCommand("DELETE FROM MatchStateHistory WHERE HistoryId = @HistoryId", conn);
                    deleteCmd.Parameters.AddWithValue("@HistoryId", historyId);
                    deleteCmd.ExecuteNonQuery();

                    // 4. Update UI and Logs
                    lblUndoStatus.Text = "Action successfully undone.";
                    lblStatus.Text = $"Score reverted to: {oldScore}/{oldWickets} in {oldOvers}.{oldBalls} overs.";

                    WriteLog(matchId, $"ACTION REMOVED: Mistaken entry undone. Score reverted to {oldScore}/{oldWickets} in {oldOvers}.{oldBalls} overs.");
                    RefreshLogs(matchId);
                }
                else
                {
                    reader.Close();
                    lblUndoStatus.Text = "Nothing left to undo for this match!";
                }
            }
        }

        protected void btnManualUpdate_Click(object sender, EventArgs e)
        {
            int matchId = GetLiveMatchId();
            if (matchId == 0)
            {
                lblUndoStatus.Text = "No live match to update!";
                return;
            }

            // 1. Validate Inputs
            int newScore, newWickets, newOvers, newBalls;
            if (!int.TryParse(txtManualScore.Text, out newScore) ||
                !int.TryParse(txtManualWickets.Text, out newWickets) ||
                !int.TryParse(txtManualOvers.Text, out newOvers) ||
                !int.TryParse(txtManualBalls.Text, out newBalls))
            {
                lblUndoStatus.Text = "Invalid input! Please enter valid numbers in all fields.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 2. Get Current State for Backup
                string getScoreQuery = "SELECT TeamScore, Wickets, Overs, Balls, Innings, TargetScore FROM MatchState WHERE MatchId = @MatchId";
                SqlCommand getCmd = new SqlCommand(getScoreQuery, conn);
                getCmd.Parameters.AddWithValue("@MatchId", matchId);
                SqlDataReader reader = getCmd.ExecuteReader();

                int currentScore = 0, currentWickets = 0, currentOvers = 0, currentBalls = 0, currentInnings = 1, targetScore = 0;
                if (reader.Read())
                {
                    currentScore = Convert.ToInt32(reader["TeamScore"]);
                    currentWickets = Convert.ToInt32(reader["Wickets"]);
                    currentOvers = Convert.ToInt32(reader["Overs"]);
                    currentBalls = Convert.ToInt32(reader["Balls"]);
                    currentInnings = Convert.ToInt32(reader["Innings"]);
                    targetScore = Convert.ToInt32(reader["TargetScore"]);
                }
                reader.Close();

                // 3. Backup to MatchStateHistory (Allows Undo to work properly)
                string backupQuery = @"
            INSERT INTO MatchStateHistory (MatchId, TeamScore, Wickets, Overs, Balls, Innings, TargetScore) 
            VALUES (@MatchId, @Score, @Wickets, @Overs, @Balls, @Innings, @Target)";
                SqlCommand backupCmd = new SqlCommand(backupQuery, conn);
                backupCmd.Parameters.AddWithValue("@MatchId", matchId);
                backupCmd.Parameters.AddWithValue("@Score", currentScore);
                backupCmd.Parameters.AddWithValue("@Wickets", currentWickets);
                backupCmd.Parameters.AddWithValue("@Overs", currentOvers);
                backupCmd.Parameters.AddWithValue("@Balls", currentBalls);
                backupCmd.Parameters.AddWithValue("@Innings", currentInnings);
                backupCmd.Parameters.AddWithValue("@Target", targetScore);
                backupCmd.ExecuteNonQuery();

                // 4. Update MatchState with the Manual Values
                string updateQuery = @"
            UPDATE MatchState 
            SET TeamScore = @NewScore, Wickets = @NewWickets, Overs = @NewOvers, Balls = @NewBalls, LastUpdated = GETDATE()
            WHERE MatchId = @MatchId";
                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@NewScore", newScore);
                updateCmd.Parameters.AddWithValue("@NewWickets", newWickets);
                updateCmd.Parameters.AddWithValue("@NewOvers", newOvers);
                updateCmd.Parameters.AddWithValue("@NewBalls", newBalls);
                updateCmd.Parameters.AddWithValue("@MatchId", matchId);
                updateCmd.ExecuteNonQuery();

                // 5. Update UI & Audit Logs
                lblUndoStatus.Text = "Manual override applied successfully!";
                lblStatus.Text = $"Score manually set to: {newScore}/{newWickets} in {newOvers}.{newBalls} overs.";

                WriteLog(matchId, $"MANUAL OVERRIDE: Score changed from {currentScore}/{currentWickets} ({currentOvers}.{currentBalls}) to {newScore}/{newWickets} ({newOvers}.{newBalls}).");
                RefreshLogs(matchId);

                // Clear input boxes after submission
                txtManualScore.Text = "";
                txtManualWickets.Text = "";
                txtManualOvers.Text = "";
                txtManualBalls.Text = "";
            }
        }


        // --- QUICK ADJUSTMENT EVENT HANDLERS ---
        protected void btnIncScore_Click(object sender, EventArgs e) { AdjustState(1, 0, 0, "+1 Score"); }
        protected void btnDecScore_Click(object sender, EventArgs e) { AdjustState(-1, 0, 0, "-1 Score"); }
        protected void btnIncBall_Click(object sender, EventArgs e) { AdjustState(0, 1, 0, "+1 Ball"); }
        protected void btnDecBall_Click(object sender, EventArgs e) { AdjustState(0, -1, 0, "-1 Ball"); }
        protected void btnIncWicket_Click(object sender, EventArgs e) { AdjustState(0, 0, 1, "+1 Wicket"); }
        protected void btnDecWicket_Click(object sender, EventArgs e) { AdjustState(0, 0, -1, "-1 Wicket"); }

        private void AdjustState(int scoreDelta, int ballDelta, int wicketDelta, string actionName)
        {
            int matchId = GetLiveMatchId();
            if (matchId == 0)
            {
                lblUndoStatus.Text = "No live match to update!";
                return;
            }

            int maxOvers = 5;
            int maxWickets = 8;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. Fetch Current State
                string getScoreQuery = "SELECT TeamScore, Wickets, Overs, Balls, Innings, TargetScore FROM MatchState WHERE MatchId = @MatchId";
                SqlCommand getCmd = new SqlCommand(getScoreQuery, conn);
                getCmd.Parameters.AddWithValue("@MatchId", matchId);
                SqlDataReader reader = getCmd.ExecuteReader();

                int currentScore = 0, currentWickets = 0, currentOvers = 0, currentBalls = 0, currentInnings = 1, targetScore = 0;
                if (reader.Read())
                {
                    currentScore = Convert.ToInt32(reader["TeamScore"]);
                    currentWickets = Convert.ToInt32(reader["Wickets"]);
                    currentOvers = Convert.ToInt32(reader["Overs"]);
                    currentBalls = Convert.ToInt32(reader["Balls"]);
                    currentInnings = Convert.ToInt32(reader["Innings"]);
                    targetScore = Convert.ToInt32(reader["TargetScore"]);
                }
                reader.Close();

                // 2. Calculate New State Values
                int newScore = currentScore + scoreDelta;
                int newWickets = currentWickets + wicketDelta;

                // Convert current overs and balls into total balls, apply delta, then convert back
                int totalBalls = (currentOvers * 6) + currentBalls + ballDelta;

                // Apply Lower Bounds (Cannot go below 0)
                if (newScore < 0) newScore = 0;
                if (newWickets < 0) newWickets = 0;
                if (totalBalls < 0) totalBalls = 0;

                // Apply Upper Bounds
                if (newWickets > maxWickets) newWickets = maxWickets;

                int newOvers = totalBalls / 6;
                int newBalls = totalBalls % 6;

                if (newOvers > maxOvers || (newOvers == maxOvers && newBalls > 0))
                {
                    newOvers = maxOvers;
                    newBalls = 0;
                }

                // Prevent updating if boundaries stopped the change (e.g. -1 score when score is 0)
                if (newScore == currentScore && newWickets == currentWickets && newOvers == currentOvers && newBalls == currentBalls)
                {
                    lblUndoStatus.Text = "Action ignored: Upper or lower limits reached.";
                    return;
                }

                // 3. Backup Current State for Undo
                string backupQuery = @"
            INSERT INTO MatchStateHistory (MatchId, TeamScore, Wickets, Overs, Balls, Innings, TargetScore) 
            VALUES (@MatchId, @Score, @Wickets, @Overs, @Balls, @Innings, @Target)";
                SqlCommand backupCmd = new SqlCommand(backupQuery, conn);
                backupCmd.Parameters.AddWithValue("@MatchId", matchId);
                backupCmd.Parameters.AddWithValue("@Score", currentScore);
                backupCmd.Parameters.AddWithValue("@Wickets", currentWickets);
                backupCmd.Parameters.AddWithValue("@Overs", currentOvers);
                backupCmd.Parameters.AddWithValue("@Balls", currentBalls);
                backupCmd.Parameters.AddWithValue("@Innings", currentInnings);
                backupCmd.Parameters.AddWithValue("@Target", targetScore);
                backupCmd.ExecuteNonQuery();

                // 4. Update Database with New State
                string updateQuery = @"
            UPDATE MatchState 
            SET TeamScore = @NewScore, Wickets = @NewWickets, Overs = @NewOvers, Balls = @NewBalls, LastUpdated = GETDATE()
            WHERE MatchId = @MatchId";
                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@NewScore", newScore);
                updateCmd.Parameters.AddWithValue("@NewWickets", newWickets);
                updateCmd.Parameters.AddWithValue("@NewOvers", newOvers);
                updateCmd.Parameters.AddWithValue("@NewBalls", newBalls);
                updateCmd.Parameters.AddWithValue("@MatchId", matchId);
                updateCmd.ExecuteNonQuery();

                // 5. Provide Feedback and Log Output
                lblUndoStatus.Text = $"{actionName} applied successfully.";
                lblStatus.Text = $"Score adjusted to: {newScore}/{newWickets} in {newOvers}.{newBalls} overs.";

                WriteLog(matchId, $"QUICK ADJUST ({actionName}): State changed to {newScore}/{newWickets} ({newOvers}.{newBalls}).");
                RefreshLogs(matchId);
            }
        }
    }
}