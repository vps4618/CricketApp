<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/CompeteTracker/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CricketApp.CompeteTracker.Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-12">
            <h2>Question Status</h2>
            
            <asp:UpdatePanel ID="DashboardUpdatePanel" runat="server">
                <ContentTemplate>
                    <!-- Triggers the UpdatePanel to refresh every 5 seconds (5000 ms) -->
                    <asp:Timer ID="DashboardTimer" runat="server" Interval="5000" OnTick="DashboardTimer_Tick"></asp:Timer>
                    
                    <div class="row">
                        <asp:Repeater ID="rptMembers" runat="server" OnItemDataBound="rptMembers_ItemDataBound">
                            <ItemTemplate>
                                <div class="col-lg-4 col-md-4 col-4 mb-2" style="min-width: 0;">

                                    <div class="card">
                                        <div class="card-header bg-primary text-white">
                                            <%# Eval("Name") %>
                                        </div>
                                        <ul class="list-group list-group-flush">
                                            <!-- Nested repeater for this member's questions -->
                                            <asp:Repeater ID="rptQuestions" runat="server">
                                                <ItemTemplate>
                                                    <li class="list-group-item d-flex justify-content-between align-items-center">
                                                        <%# Eval("QuestionTitle") %>
                                                        <span class="badge bg-<%# GetStatusColor(Eval("Status").ToString()) %> rounded-pill">
                                                            <%# Eval("Status") %>
                                                        </span>
                                                    </li>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </ul>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    
                </ContentTemplate>
            </asp:UpdatePanel>
            
        </div>
    </div>
</asp:Content>