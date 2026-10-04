<%@ Page Title="" Language="C#" MasterPageFile="~/CompeteTracker/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CricketApp.CompeteTracker.Login" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center mt-5">
        <div class="col-md-4">
            <div class="card shadow">
                <div class="card-header bg-primary text-white text-center">
                    <h4>Member Login</h4>
                </div>
                <div class="card-body">
                    <asp:Label ID="lblError" runat="server" CssClass="text-danger d-block mb-2"></asp:Label>
                    
                    <div class="mb-3">
                        <label class="form-label">Username (Your Name)</label>
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" placeholder="e.g. Vishwa"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Team Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    </div>
                    
                    <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary w-100" OnClick="btnLogin_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
