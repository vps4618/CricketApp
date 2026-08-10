<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="CricketApp.Admin" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Scorer Dashboard</title>
    <style>
        body {
            font-family: Arial;
            padding: 20px;
        }

        .btn {
            padding: 15px 25px;
            margin: 5px;
            font-size: 16px;
            cursor: pointer;
        }

        .btn-run {
            background-color: #4CAF50;
            color: white;
            border: none;
        }

        .btn-extra {
            background-color: #FF9800;
            color: white;
            border: none;
        }

        .btn-out {
            background-color: #f44336;
            color: white;
            border: none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">

        <!-- MAIN FLEX CONTAINER -->
        <div style="display: flex; gap: 30px; flex-wrap: wrap; align-items: flex-start;">

            <!-- ========================================== -->
            <!-- LEFT COLUMN: ADMIN CONTROLS                -->
            <!-- ========================================== -->
            <div style="flex: 2; min-width: 400px;">
                <h2>Admin Dashboard - Update Score</h2>

                <!-- TOURNAMENT MANAGEMENT SECTION -->
                <div style="background-color: #e3f2fd; padding: 20px; border-radius: 8px; margin-bottom: 20px; border: 1px solid #90caf9;">
                    <h3 style="margin-top: 0;">Start a New Match</h3>
                    Batting Team: 
                    <asp:DropDownList ID="ddlBattingTeam" runat="server" DataTextField="TeamName" DataValueField="TeamId"></asp:DropDownList>
                    &nbsp; vs &nbsp; Bowling Team: 
                    <asp:DropDownList ID="ddlBowlingTeam" runat="server" DataTextField="TeamName" DataValueField="TeamId"></asp:DropDownList>
                    <br />
                    <br />
                    <!-- ADDED TOSS WINNER -->
                    Toss Winner:
                    <asp:DropDownList ID="ddlTossWinner" runat="server" DataTextField="TeamName" DataValueField="TeamId"></asp:DropDownList>
                    <br /><br />
                    <asp:Button ID="btnStartMatch" runat="server" Text="Start Match" OnClick="btnStartMatch_Click" BackColor="#2196F3" ForeColor="White" BorderStyle="None" Padding="10" />
                    <asp:Button ID="btnSecondInnings" runat="server" Text="Start 2nd Innings (Swap Teams)" OnClick="btnSecondInnings_Click" BackColor="#FF9800" ForeColor="White" BorderStyle="None" Padding="10" />
                    <!--
                    <hr style="border: 1px solid #bbdefb; margin: 15px 0;" />

                    <h3 style="margin-top: 0;">2. End Current Match</h3>
                    Select Winner: 
                    <asp:DropDownList ID="ddlWinner" runat="server" DataTextField="TeamName" DataValueField="TeamId"></asp:DropDownList>
                    <asp:Button ID="btnEndMatch" runat="server" Text="Save Winner & End Match" OnClick="btnEndMatch_Click" BackColor="#9C27B0" ForeColor="White" BorderStyle="None" Padding="10" />-->
                    <br />
                    <br />
                    <asp:Label ID="lblMatchMessage" runat="server" Font-Bold="True" ForeColor="#d32f2f"></asp:Label>
                </div>

                <!-- SCORING BUTTONS -->
                <div>
                    <h3>Runs (Legal Deliveries)</h3>
                    <asp:Button ID="btnDot" runat="server" Text="Dot Ball (0)" CssClass="btn btn-run" OnClick="btnDot_Click" />
                    <asp:Button ID="btn1Run" runat="server" Text="1 Run" CssClass="btn btn-run" OnClick="btn1Run_Click" />
                    <asp:Button ID="btn2Runs" runat="server" Text="2 Runs" CssClass="btn btn-run" OnClick="btn2Runs_Click" />
                    <!-- ADDED 3 RUNS -->
                    <asp:Button ID="btn3Runs" runat="server" Text="3 Runs" CssClass="btn btn-run" OnClick="btn3Runs_Click" />
                    <asp:Button ID="btnFour" runat="server" Text="FOUR (4)" CssClass="btn btn-run" OnClick="btnFour_Click" />
                    <!-- ADDED 5 RUNS -->
                    <asp:Button ID="btn5Runs" runat="server" Text="5 Runs" CssClass="btn btn-run" OnClick="btn5Runs_Click" />
                    <asp:Button ID="btnSix" runat="server" Text="SIX (6)" CssClass="btn btn-run" OnClick="btnSix_Click" />
                </div>

                <div>
                    <h3>Extras</h3>
                    <asp:Button ID="btnWide" runat="server" Text="Wide (+1)" CssClass="btn btn-extra" OnClick="btnWide_Click" />
                    <asp:Button ID="btnNoBall" runat="server" Text="No Ball (+1)" CssClass="btn btn-extra" OnClick="btnNoBall_Click" />
                </div>

                <div>
                    <h3>Wickets</h3>
                    <asp:Button ID="btnWicket" runat="server" Text="Wicket" CssClass="btn btn-out" OnClick="btnWicket_Click" />
                </div>

                <br />
                <asp:Label ID="lblStatus" runat="server" Font-Bold="True" ForeColor="Blue"></asp:Label>

                <!-- DANGER ZONE (UNDO & MANUAL OVERRIDE) -->
                <div style="margin-top: 20px; padding-top: 20px; border-top: 2px dashed #ccc;">
                    <h3 style="margin-top: 0; color: #d32f2f;">Danger Zone</h3>
                    <asp:Button ID="btnUndo" runat="server" Text="⚠️ Undo Latest Action" OnClick="btnUndo_Click" BackColor="#607D8B" ForeColor="White" BorderStyle="None" Padding="15" />
                    <br />
                    <br />
                    <!-- ADDED QUICK ADJUSTMENTS -->
                    <h4 style="color: #d32f2f; margin-bottom: 5px;">Quick Adjustments (Limits Enforced)</h4>
                    <div style="display: flex; gap: 10px; margin-bottom: 20px; flex-wrap: wrap;">
                        <asp:Button ID="btnIncScore" runat="server" Text="+1 Score" OnClick="btnIncScore_Click" BackColor="#4CAF50" ForeColor="White" BorderStyle="None" Padding="8" />
                        <asp:Button ID="btnDecScore" runat="server" Text="-1 Score" OnClick="btnDecScore_Click" BackColor="#f44336" ForeColor="White" BorderStyle="None" Padding="8" />

                        <asp:Button ID="btnIncBall" runat="server" Text="+1 Ball" OnClick="btnIncBall_Click" BackColor="#4CAF50" ForeColor="White" BorderStyle="None" Padding="8" />
                        <asp:Button ID="btnDecBall" runat="server" Text="-1 Ball" OnClick="btnDecBall_Click" BackColor="#f44336" ForeColor="White" BorderStyle="None" Padding="8" />

                        <asp:Button ID="btnIncWicket" runat="server" Text="+1 Wicket" OnClick="btnIncWicket_Click" BackColor="#4CAF50" ForeColor="White" BorderStyle="None" Padding="8" />
                        <asp:Button ID="btnDecWicket" runat="server" Text="-1 Wicket" OnClick="btnDecWicket_Click" BackColor="#f44336" ForeColor="White" BorderStyle="None" Padding="8" />
                    </div>
                    <br />
                    <!-- ADDED MANUAL OVERRIDE SECTION -->
                    <h4 style="color: #d32f2f; margin-bottom: 5px;">Manual State Override</h4>
                    <div style="display: flex; gap: 10px; margin-bottom: 10px; flex-wrap: wrap;">
                        <div>
                            Runs:
                            <asp:TextBox ID="txtManualScore" runat="server" Width="50" TextMode="Number"></asp:TextBox>
                        </div>
                        <div>
                            Wickets:
                            <asp:TextBox ID="txtManualWickets" runat="server" Width="50" TextMode="Number"></asp:TextBox>
                        </div>
                        <div>
                            Overs:
                            <asp:TextBox ID="txtManualOvers" runat="server" Width="50" TextMode="Number"></asp:TextBox>
                        </div>
                        <div>
                            Balls:
                            <asp:TextBox ID="txtManualBalls" runat="server" Width="50" TextMode="Number"></asp:TextBox>
                        </div>
                    </div>
                    <asp:Button ID="btnManualUpdate" runat="server" Text="⚠️ Force Update State" OnClick="btnManualUpdate_Click" BackColor="#f44336" ForeColor="White" BorderStyle="None" Padding="10" />

                    <br />
                    <br />
                    <asp:Label ID="lblUndoStatus" runat="server" Font-Bold="True" ForeColor="#d32f2f"></asp:Label>
                </div>
            </div>

            <!-- ========================================== -->
            <!-- RIGHT COLUMN: AUDIT LOG                    -->
            <!-- ========================================== -->
            <div style="flex: 1; min-width: 350px;">
                <div style="background-color: #f5f5f5; padding: 15px; border: 1px solid #ddd; border-radius: 5px; position: sticky; top: 20px;">
                    <h3 style="margin-top: 0; color: #333;">Live Match Audit Log</h3>

                    <asp:TextBox ID="txtActivityLog" runat="server" TextMode="MultiLine" ReadOnly="true"
                        Style="width: 100%; height: 75vh; background-color: #fff; color: #333; font-family: 'Courier New', Courier, monospace; font-size: 14px; padding: 10px; border: 1px solid #ccc; resize: none; box-sizing: border-box;">
                    </asp:TextBox>
                </div>
            </div>

        </div>
    </form>
</body>
</html>
