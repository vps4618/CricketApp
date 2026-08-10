<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageMatchState.aspx.cs" Inherits="CricketApp.ManageMatchState" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Manage Match State</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
            background-color: #f4f4f9;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.1);
        }
        .btn {
            padding: 10px 15px;
            cursor: pointer;
            border: none;
            border-radius: 4px;
            color: white;
        }
        .btn-add { background-color: #4CAF50; font-size: 14px; margin-top: 10px; }
        .btn-back { 
            background-color: #2196F3; 
            text-decoration: none; 
            display: inline-block; 
            margin-bottom: 20px;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        th, td {
            border: 1px solid #ddd;
            padding: 10px;
            text-align: left;
        }
        th { background-color: #f2f2f2; }
        .action-links a { margin-right: 10px; text-decoration: none; color: #1976D2; }
        .form-row { display: flex; gap: 15px; margin-bottom: 10px; flex-wrap: wrap; }
        .form-group { display: flex; flex-direction: column; width: 150px; }
        .form-group label { font-weight: bold; margin-bottom: 5px; font-size: 14px; }
        .form-group input, .form-group select { padding: 5px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <a href="admin" class="btn btn-back">← Back to Score Dashboard</a>
            
            <h2 style="margin-top: 0;">Manage Match States</h2>

            <!-- ADD NEW MATCH STATE SECTION -->
            <div style="margin-bottom: 20px; padding: 15px; background: #e3f2fd; border-radius: 5px; border: 1px solid #90caf9;">
                <h3 style="margin-top: 0;">Create Initial Match State</h3>
                <p style="font-size: 13px; color: #555;">Note: Each Match ID can only have one active state record.</p>
                
                <div class="form-row">
                    <div class="form-group">
                        <label>Match ID:</label>
                        <asp:DropDownList ID="ddlMatchId" runat="server" DataTextField="MatchId" DataValueField="MatchId"></asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <label>Batting Team:</label>
                        <asp:DropDownList ID="ddlBattingTeamId" runat="server" DataTextField="TeamName" DataValueField="TeamId"></asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <label>Innings:</label>
                        <asp:TextBox ID="txtInnings" runat="server" TextMode="Number" Text="1"></asp:TextBox>
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-group">
                        <label>Team Score:</label>
                        <asp:TextBox ID="txtTeamScore" runat="server" TextMode="Number" Text="0"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Wickets:</label>
                        <asp:TextBox ID="txtWickets" runat="server" TextMode="Number" Text="0"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Overs:</label>
                        <asp:TextBox ID="txtOvers" runat="server" TextMode="Number" Text="0"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Balls:</label>
                        <asp:TextBox ID="txtBalls" runat="server" TextMode="Number" Text="0"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Target Score:</label>
                        <asp:TextBox ID="txtTargetScore" runat="server" TextMode="Number" Text="0"></asp:TextBox>
                    </div>
                </div>
                
                <asp:Button ID="btnAddMatchState" runat="server" Text="Create Match State" CssClass="btn btn-add" OnClick="btnAddMatchState_Click" />
                <br /><br />
                <asp:Label ID="lblMessage" runat="server" Font-Bold="true"></asp:Label>
            </div>

            <!-- VIEW, UPDATE, DELETE MATCH STATE GRIDVIEW -->
            <div style="overflow-x: auto;">
                <asp:GridView ID="gvMatchState" runat="server" AutoGenerateColumns="False" DataKeyNames="MatchId"
                    OnRowEditing="gvMatchState_RowEditing" OnRowCancelingEdit="gvMatchState_RowCancelingEdit"
                    OnRowUpdating="gvMatchState_RowUpdating" OnRowDeleting="gvMatchState_RowDeleting"
                    GridLines="None" CssClass="grid-view">
                    <Columns>
                        <asp:BoundField DataField="MatchId" HeaderText="Match ID" ReadOnly="True" ItemStyle-Width="70px" />
                        
                        <asp:BoundField DataField="BattingTeamId" HeaderText="Batting Team ID" ItemStyle-Width="120px" />
                        <asp:BoundField DataField="TeamScore" HeaderText="Score" ItemStyle-Width="60px" />
                        <asp:BoundField DataField="Wickets" HeaderText="Wickets" ItemStyle-Width="60px" />
                        <asp:BoundField DataField="Overs" HeaderText="Overs" ItemStyle-Width="60px" />
                        <asp:BoundField DataField="Balls" HeaderText="Balls" ItemStyle-Width="60px" />
                        <asp:BoundField DataField="Innings" HeaderText="Innings" ItemStyle-Width="60px" />
                        <asp:BoundField DataField="TargetScore" HeaderText="Target" ItemStyle-Width="70px" />
                        <asp:BoundField DataField="LastUpdated" HeaderText="Last Updated" ReadOnly="True" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                        
                        <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" HeaderText="Actions" ItemStyle-Width="150px" ItemStyle-CssClass="action-links" />
                    </Columns>
                </asp:GridView>
                <small style="color: gray; margin-top: 10px; display: block;">* Note: Last Updated is generated automatically upon save or update.</small>
            </div>
        </div>
    </form>
</body>
</html>