-- 1. Store all your tournament teams
CREATE TABLE Teams (
    TeamId INT IDENTITY(1,1) PRIMARY KEY,
    TeamName NVARCHAR(100)
);

-- 2. Store the tournament matches and track the winner
CREATE TABLE Matches (
    MatchId INT IDENTITY(1,1) PRIMARY KEY,
    Team1Id INT FOREIGN KEY REFERENCES Teams(TeamId),
    Team2Id INT FOREIGN KEY REFERENCES Teams(TeamId),
    WinnerTeamId INT NULL, -- Left NULL until the match finishes
    IsLive BIT DEFAULT 0   -- We set this to 1 for the match currently being played
);

-- 3. The live scoreboard (Notice we added LastUpdated)
CREATE TABLE MatchState (
    MatchId INT PRIMARY KEY FOREIGN KEY REFERENCES Matches(MatchId),
    BattingTeamId INT FOREIGN KEY REFERENCES Teams(TeamId),
    TeamScore INT,
    Wickets INT,
    Overs INT,
    Balls INT,
    LastUpdated DATETIME,
    Innings INT DEFAULT 1,
    TargetScore INT DEFAULT 0
);

-- 4. sql query for creating logs
CREATE TABLE MatchLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    MatchId INT FOREIGN KEY REFERENCES Matches(MatchId),
    LogMessage NVARCHAR(500),
    LogTime DATETIME DEFAULT GETDATE()
);

CREATE TABLE MatchStateHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    MatchId INT FOREIGN KEY REFERENCES Matches(MatchId),
    TeamScore INT,
    Wickets INT,
    Overs INT,
    Balls INT,
    Innings INT,
    TargetScore INT
);