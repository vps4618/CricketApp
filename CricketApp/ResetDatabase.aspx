<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetDatabase.aspx.cs" Inherits="CricketApp.ResetDatabase" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>DANGER: Reset Database</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
            background-color: #f4f4f9;
        }
        .container {
            max-width: 600px;
            margin: 50px auto;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
            text-align: center;
        }
        .danger-zone {
            border: 2px dashed #f44336;
            padding: 20px;
            background-color: #ffebee;
            border-radius: 8px;
            margin-bottom: 20px;
        }
        h2 {
            color: #d32f2f;
            margin-top: 0;
        }
        .btn {
            padding: 12px 20px;
            cursor: pointer;
            border: none;
            border-radius: 4px;
            color: white;
            font-size: 16px;
            font-weight: bold;
        }
        .btn-danger { background-color: #f44336; margin-top: 15px; }
        .btn-danger:disabled { background-color: #e57373; cursor: not-allowed; }
        .btn-back { 
            background-color: #607D8B; 
            text-decoration: none; 
            display: inline-block; 
            margin-top: 20px;
        }
        .warning-text {
            color: #b71c1c;
            font-size: 15px;
            font-weight: bold;
            margin-bottom: 15px;
        }
    </style>
    <script>
        function toggleDeleteButton() {
            var checkBox = document.getElementById("chkConfirm");
            var btnDelete = document.getElementById("btnDeleteAll");
            btnDelete.disabled = !checkBox.checked;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="danger-zone">
                <h2>⚠️ DANGER ZONE: Factory Reset</h2>
                <p class="warning-text">
                    This action will DROP ALL TABLES and wipe all data, including teams, matches, states, logs, and histories. This CANNOT be undone!
                </p>
                
                <div style="margin: 20px 0; text-align: left; background: white; padding: 10px; border-radius: 5px; border: 1px solid #ffcdd2;">
                    <input type="checkbox" id="chkConfirm" onclick="toggleDeleteButton()" />
                    <label for="chkConfirm" style="color: #d32f2f; font-weight: bold; cursor: pointer;">
                        I understand that this will permanently delete all data in the database.
                    </label>
                </div>

                <asp:Button ID="btnDeleteAll" runat="server" Text="Drop All Tables & Wipe Database" 
                    CssClass="btn btn-danger" Enabled="false" ClientIDMode="Static" OnClick="btnDeleteAll_Click" />
                
                <br /><br />
                <asp:Label ID="lblMessage" runat="server" Font-Bold="true" Font-Size="16px"></asp:Label>
            </div>

            <a href="admin" class="btn btn-back">← Go Back to Safety (Dashboard)</a>
        </div>
    </form>
</body>
</html>