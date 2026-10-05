<%@ Page Title="Reset Database" Language="C#" MasterPageFile="~/CompeteTracker/Site.Master" AutoEventWireup="true" CodeBehind="ResetDatabase.aspx.cs" Inherits="CricketApp.CompeteTracker.ResetDatabase" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center mt-5">
        <div class="col-md-6">
            <div class="card shadow">
                <div class="card-header bg-danger text-white text-center">
                    <h4>⚠️ Reset Database</h4>
                </div>
                <div class="card-body">
                    <p class="text-muted mb-4">
                        <strong>Warning:</strong> This will delete all data and recreate empty tables. This action cannot be undone!
                    </p>

                    <asp:Label ID="lblMessage" runat="server" CssClass="d-block mb-3 font-weight-bold"></asp:Label>

                    <div class="d-grid gap-2">
                        <asp:Button ID="btnResetDatabase" runat="server" Text="Reset Database" CssClass="btn btn-danger btn-lg" OnClick="btnResetDatabase_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary btn-lg" PostBackUrl="Default.aspx" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>