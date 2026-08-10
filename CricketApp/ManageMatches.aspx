<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageMatches.aspx.cs" Inherits="CricketApp.ManageMatches" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Manage Matches</title>
        <style>
        iframe[src*="somee.com"],
        script[src*="somee.com"],
        a[href*="somee.com"],
        div[style*="z-index"] {
            display: none !important;
            visibility: hidden !important;
        }
    </style>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
            background-color: #f4f4f9;
        }
        .container {
            max-width: 900px;
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
        .form-group { margin-bottom: 10px; }
        .form-group label { display: inline-block; width: 120px; font-weight: bold; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <a href="admin" class="btn btn-back">← Back to Score Dashboard</a>
            
            <h2 style="margin-top: 0;">Manage Tournament Matches</h2>

            <!-- ADD NEW MATCH SECTION -->
            <div style="margin-bottom: 20px; padding: 15px; background: #e3f2fd; border-radius: 5px; border: 1px solid #90caf9;">
                <h3 style="margin-top: 0;">Create New Match</h3>
                
                <div class="form-group">
                    <label>Team 1 (Batting):</label>
                    <asp:DropDownList ID="ddlTeam1" runat="server" DataTextField="TeamName" DataValueField="TeamId"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Team 2 (Bowling):</label>
                    <asp:DropDownList ID="ddlTeam2" runat="server" DataTextField="TeamName" DataValueField="TeamId"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Toss Winner:</label>
                    <asp:DropDownList ID="ddlTossWinner" runat="server" DataTextField="TeamName" DataValueField="TeamId"></asp:DropDownList>
                </div>
                
                <asp:Button ID="btnAddMatch" runat="server" Text="Create Match" CssClass="btn btn-add" OnClick="btnAddMatch_Click" />
                <br /><br />
                <asp:Label ID="lblMessage" runat="server" Font-Bold="true"></asp:Label>
            </div>

            <!-- VIEW, UPDATE, DELETE MATCHES GRIDVIEW -->
            <div style="overflow-x: auto;">
                <asp:GridView ID="gvMatches" runat="server" AutoGenerateColumns="False" DataKeyNames="MatchId"
                    OnRowEditing="gvMatches_RowEditing" OnRowCancelingEdit="gvMatches_RowCancelingEdit"
                    OnRowUpdating="gvMatches_RowUpdating" OnRowDeleting="gvMatches_RowDeleting"
                    GridLines="None" CssClass="grid-view">
                    <Columns>
                        <asp:BoundField DataField="MatchId" HeaderText="Match ID" ReadOnly="True" ItemStyle-Width="70px" />
                        
                        <asp:BoundField DataField="Team1Id" HeaderText="Team 1 ID" />
                        <asp:BoundField DataField="Team2Id" HeaderText="Team 2 ID" />
                        <asp:BoundField DataField="TossWinnerId" HeaderText="Toss Winner ID" />
                        <asp:BoundField DataField="WinnerTeamId" HeaderText="Winner ID" />
                        
                        <asp:CheckBoxField DataField="IsLive" HeaderText="Is Live" />
                        
                        <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" HeaderText="Actions" ItemStyle-Width="150px" ItemStyle-CssClass="action-links" />
                    </Columns>
                </asp:GridView>
                <small style="color: gray; margin-top: 10px; display: block;">* Note: Use Team IDs when editing records directly in the grid.</small>
            </div>
        </div>
    </form>
        <!--script to remove ads-->
    <script>
        window.onload = function () {

            // Remove all scripts containing "somee.com"
            document.querySelectorAll("script").forEach(script => {
                if (script.src.includes("somee.com") || script.innerHTML.includes("somee.com")) {
                    script.remove();
                }
            });

            // Remove the "Web hosting by Somee.com" banner
            document.querySelectorAll("a[href='http://somee.com']").forEach(link => {
                link.parentElement.remove();
            });

            // Remove injected comments
            document.body.childNodes.forEach(node => {
                if (node.nodeType === Node.COMMENT_NODE && node.nodeValue.includes("SCRIPT GENERATED BY SERVER")) {
                    node.remove();
                }
            });
        };
    </script>
</body>
</html>