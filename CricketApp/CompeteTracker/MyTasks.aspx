<%@ Page Title="My Tasks" Language="C#" MasterPageFile="~/CompeteTracker/Site.Master" AutoEventWireup="true" CodeBehind="MyTasks.aspx.cs" Inherits="CricketApp.CompeteTracker.MyTasks" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-12 mb-4">
            <h2>Welcome, <asp:Label ID="lblMemberName" runat="server" CssClass="text-primary"></asp:Label></h2>
            <hr />
        </div>

        <!-- ADD A QUESTION FOR MYSELF -->
        <div class="col-lg-4 col-4 mb-4"> 
            <div class="card shadow-sm">
                <div class="card-header bg-info text-dark">Add New Question For Me</div>
                <div class="card-body">
                    <!-- NEW: Label for error/success messages -->
                    <asp:Label ID="lblAddMessage" runat="server" CssClass="d-block mb-2 font-weight-bold"></asp:Label>

                    <div class="mb-3">
                        <label class="form-label">Question Title</label>
                        <asp:TextBox ID="txtMyNewQuestion" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <asp:Button ID="btnAddMyQuestion" runat="server" Text="Add & Assign to Me" CssClass="btn btn-info w-100" OnClick="btnAddMyQuestion_Click" />
                </div>
            </div>
        </div>

        <!-- MANAGE MY QUESTIONS -->
        <div class="col-lg-8 col-8 mb-4"> 
            <div class="card shadow-sm">
                <div class="card-header bg-secondary text-white">Update My Question Status</div>
                <div class="card-body">
                    <div class="table-responsive">
                        <asp:GridView ID="gridMyQuestions" runat="server" AutoGenerateColumns="False" DataKeyNames="QuestionId" DataSourceID="SqlDataSourceMyQuestions" CssClass="table table-striped table-hover" GridLines="None">
                            <Columns>
                                <asp:CommandField ShowEditButton="True" ControlStyle-CssClass="btn btn-sm btn-outline-secondary me-1" />
                                <asp:BoundField DataField="QuestionTitle" HeaderText="Title" ReadOnly="True" />
                                
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class="badge bg-<%# GetStatusColor(Eval("Status").ToString()) %>">
                                            <%# Eval("Status") %>
                                        </span>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="ddlMyStatus" runat="server" SelectedValue='<%# Bind("Status") %>' CssClass="form-select form-select-sm">
                                            <asp:ListItem>Pending</asp:ListItem>
                                            <asp:ListItem>Ongoing</asp:ListItem>
                                            <asp:ListItem>Finished</asp:ListItem>
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

    <!-- SqlDataSource filtered by Session["MemberId"] -->
    <asp:SqlDataSource ID="SqlDataSourceMyQuestions" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CricketDb %>" 
        SelectCommand="SELECT QuestionId,QuestionTitle, Status FROM [Questions] WHERE AssignedMemberId = @MemberId ORDER BY ISNULL(ModifiedDate, CreatedDate) DESC"

        UpdateCommand="UPDATE [Questions] SET [Status] = @Status, [ModifiedDate] = GETUTCDATE() WHERE [QuestionId] = @QuestionId">

        <SelectParameters>
            <asp:SessionParameter Name="MemberId" SessionField="MemberId" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
