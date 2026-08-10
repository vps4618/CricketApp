<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageLogs.aspx.cs" Inherits="CricketApp.ManageLogs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Manage Match Logs</title>
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
            max-width: 1000px;
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
        .form-group { margin-bottom: 10px; display: flex; align-items: center; }
        .form-group label { display: inline-block; width: 120px; font-weight: bold; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <a href="admin" class="btn btn-back">← Back to Score Dashboard</a>
            
            <h2 style="margin-top: 0;">Manage Match Logs</h2>

            <!-- ADD NEW LOG SECTION -->
            <div style="margin-bottom: 20px; padding: 15px; background: #e3f2fd; border-radius: 5px; border: 1px solid #90caf9;">
                <h3 style="margin-top: 0;">Add Manual Log Entry</h3>
                
                <div class="form-group">
                    <label>Match ID:</label>
                    <asp:DropDownList ID="ddlMatchId" runat="server" DataTextField="MatchId" DataValueField="MatchId"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Log Message:</label>
                    <asp:TextBox ID="txtLogMessage" runat="server" Width="400px" MaxLength="500"></asp:TextBox>
                </div>
                
                <asp:Button ID="btnAddLog" runat="server" Text="Add Log Entry" CssClass="btn btn-add" OnClick="btnAddLog_Click" />
                <br /><br />
                <asp:Label ID="lblMessage" runat="server" Font-Bold="true"></asp:Label>
            </div>

            <!-- VIEW, UPDATE, DELETE LOGS GRIDVIEW -->
            <div style="overflow-x: auto;">
                <asp:GridView ID="gvLogs" runat="server" AutoGenerateColumns="False" DataKeyNames="LogId"
                    OnRowEditing="gvLogs_RowEditing" OnRowCancelingEdit="gvLogs_RowCancelingEdit"
                    OnRowUpdating="gvLogs_RowUpdating" OnRowDeleting="gvLogs_RowDeleting"
                    GridLines="None" CssClass="grid-view">
                    <Columns>
                        <asp:BoundField DataField="LogId" HeaderText="Log ID" ReadOnly="True" ItemStyle-Width="70px" />
                        
                        <asp:BoundField DataField="MatchId" HeaderText="Match ID" ItemStyle-Width="80px" />
                        <asp:BoundField DataField="LogMessage" HeaderText="Log Message" />
                        <asp:BoundField DataField="LogTime" HeaderText="Log Time" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                        
                        <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" HeaderText="Actions" ItemStyle-Width="150px" ItemStyle-CssClass="action-links" />
                    </Columns>
                </asp:GridView>
                <small style="color: gray; margin-top: 10px; display: block;">* Note: LogTime format should be standard datetime (e.g., YYYY-MM-DD HH:MM:SS) if you edit it manually.</small>
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