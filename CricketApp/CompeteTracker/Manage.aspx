<%@ Page Title="" Language="C#" MasterPageFile="~/CompeteTracker/Site.Master" AutoEventWireup="true" CodeBehind="Manage.aspx.cs" Inherits="CricketApp.CompeteTracker.Manage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <!-- MEMBERS SECTION -->
        <div class="col-md-5 mb-4">
            <div class="card shadow-sm">
                <div class="card-header bg-secondary text-white">Add Member</div>
                <div class="card-body">
                    <div class="input-group mb-3">
                        <asp:TextBox ID="txtNewMember" runat="server" CssClass="form-control" placeholder="Member Name"></asp:TextBox>
                        <asp:Button ID="btnAddMember" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddMember_Click" />
                    </div>
                    
                    <div class="table-responsive">
                        <asp:GridView ID="gridMembers" runat="server" AutoGenerateColumns="False" DataKeyNames="MemberId" DataSourceID="SqlDataSourceMembers" CssClass="table table-striped table-hover mt-3" GridLines="None">
                            <Columns>
                                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" ControlStyle-CssClass="btn btn-sm btn-outline-secondary me-1" />
                                <asp:BoundField DataField="MemberId" HeaderText="ID" ReadOnly="True" InsertVisible="False" SortExpression="MemberId" />
                                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>

        <!-- QUESTIONS SECTION -->
        <div class="col-md-7 mb-4">
            <div class="card shadow-sm">
                <div class="card-header bg-secondary text-white">Add Question</div>
                <div class="card-body">
                    <div class="row g-2 mb-3">
                        <div class="col-sm-8">
                            <asp:TextBox ID="txtNewQuestion" runat="server" CssClass="form-control" placeholder="Question Title"></asp:TextBox>
                        </div>
                        <div class="col-sm-4">
                            <asp:Button ID="btnAddQuestion" runat="server" Text="Add Question" CssClass="btn btn-primary w-100" OnClick="btnAddQuestion_Click" />
                        </div>
                    </div>

                    <div class="table-responsive">
                        <asp:GridView ID="gridQuestions" runat="server" AutoGenerateColumns="False" DataKeyNames="QuestionId" DataSourceID="SqlDataSourceQuestions" CssClass="table table-striped table-hover mt-3" GridLines="None">
                            <Columns>
                                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" ControlStyle-CssClass="btn btn-sm btn-outline-secondary me-1" />
                                <asp:BoundField DataField="QuestionTitle" HeaderText="Title" SortExpression="QuestionTitle" />
                                
                                <asp:TemplateField HeaderText="Status" SortExpression="Status">
                                    <ItemTemplate><%# Eval("Status") %></ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="ddlStatusGrid" runat="server" SelectedValue='<%# Bind("Status") %>' CssClass="form-select form-select-sm">
                                            <asp:ListItem>Pending</asp:ListItem>
                                            <asp:ListItem>Ongoing</asp:ListItem>
                                            <asp:ListItem>Finished</asp:ListItem>
                                        </asp:DropDownList>
                                    </EditItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Assigned To">
                                    <ItemTemplate><%# Eval("AssigneeName") %></ItemTemplate>
                                    <EditItemTemplate>
                                        <!-- Binds the Member ID for editing -->
                                        <asp:DropDownList ID="ddlMemberGrid" runat="server" DataSourceID="SqlDataSourceMembers" DataTextField="Name" DataValueField="MemberId" SelectedValue='<%# Bind("AssignedMemberId") %>' AppendDataBoundItems="true" CssClass="form-select form-select-sm">
                                            <asp:ListItem Text="-- Unassigned --" Value=""></asp:ListItem>
                                        </asp:DropDownList>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Data Sources for the GridViews -->
    <asp:SqlDataSource ID="SqlDataSourceMembers" runat="server"
        ConnectionString="<%$ ConnectionStrings:CricketDb %>"
        SelectCommand="SELECT * FROM [Members]"
        UpdateCommand="UPDATE [Members] SET [Name] = @Name WHERE [MemberId] = @MemberId"
        DeleteCommand="UPDATE [Questions] SET AssignedMemberId = NULL WHERE AssignedMemberId = @MemberId; DELETE FROM [Members] WHERE [MemberId] = @MemberId"></asp:SqlDataSource>

    <asp:SqlDataSource ID="SqlDataSourceQuestions" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CricketDb %>" 
        SelectCommand="SELECT q.QuestionId, q.QuestionTitle, q.Status, q.AssignedMemberId, m.Name as AssigneeName FROM [Questions] q LEFT JOIN [Members] m ON q.AssignedMemberId = m.MemberId" 
        UpdateCommand="UPDATE [Questions] SET [QuestionTitle] = @QuestionTitle, [Status] = @Status, [AssignedMemberId] = @AssignedMemberId, [ModifiedDate] = GETUTCDATE() WHERE [QuestionId] = @QuestionId"
        DeleteCommand="DELETE FROM [Questions] WHERE [QuestionId] = @QuestionId">
    </asp:SqlDataSource>

</asp:Content>