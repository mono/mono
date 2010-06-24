<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>

    <script runat="server">

        protected void Page_Load(object sender, EventArgs e)
        {
            FormView1.DataKeyNames = new string[] { "ID" };
            if (!IsPostBack)
                MonoTests.System.Web.UI.WebControls.FormViewDataObject.ds = MonoTests.System.Web.UI.WebControls.FormViewDataObject.CreateDataTable();
        }    
    
    </script>

    <form id="form1" runat="server">
        <div>
            <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:FormView ID="FormView1" runat="server" AllowPaging="True" DataSourceID="ObjectDataSource1">
                <ItemTemplate>
                    <asp:Label ID="ID" runat="server" Text='<%# Eval("ID") %>'></asp:Label>&nbsp;
                    <asp:Label ID="LName" runat="server" Text='<%# Eval("LName") %>'></asp:Label>
                    <asp:Label ID="FName" runat="server" Text='<%# Eval("FName") %>'></asp:Label>&nbsp;
                    <asp:LinkButton ID="EditButton" runat="server" CommandName='<%# "Edit" %>' Text='<%# "Edit" %>'></asp:LinkButton>
                    <asp:LinkButton ID="NewButton" runat="server" CommandName='<%# "New" %>' Text='<%# "New" %>'></asp:LinkButton>
                    <asp:LinkButton ID="DeleteButton" runat="server" CommandName='<%# "Delete" %>' Text='<%# "Delete" %>'></asp:LinkButton>
                </ItemTemplate>
                <EditItemTemplate>
                    Enter First Name:<asp:TextBox ID="FNameEdit" Text='<%# Bind("FName") %>' runat="server"></asp:TextBox><br />
                    Enter Last Name:<asp:TextBox ID="LNameEdit" runat="server" Text='<%# Bind("LName") %>'></asp:TextBox><br />
                    <asp:LinkButton ID="UpdateButton" runat="server" CommandName='<%# "Update" %>' Text='<%# "Update" %>'></asp:LinkButton>
                    <asp:LinkButton ID="CancelUpdateButton" runat="server" CommandName='<%# "Cancel" %>'
                        Text='<%# "Cancel" %>'></asp:LinkButton>
                </EditItemTemplate>
                <InsertItemTemplate>
                    Insert ID:
                    <asp:TextBox ID="IDInsert" runat="server" Text='<%# Bind("ID") %>'></asp:TextBox><br />
                    Insert First Name:
                    <asp:TextBox ID="FNameInsert" runat="server" Text='<%# Bind("FName") %>'></asp:TextBox>
                    <br />
                    Insert Last Name:&nbsp;
                    <asp:TextBox ID="LNameInsert" runat="server" Text='<%# Bind("LName") %>'></asp:TextBox>
                    <asp:LinkButton ID="InsertButton" runat="server" CommandName='<%# "Insert" %>' Text='<%# "Insert" %>'></asp:LinkButton>
                    <asp:LinkButton ID="CancelInsertButton" runat="server" CommandName='<%# "Cancel" %>'
                        Text='<%# "Cancel" %>'></asp:LinkButton>
                </InsertItemTemplate>
                <EmptyDataTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# "The Database is empty" %>'></asp:Label>
                </EmptyDataTemplate>
            </asp:FormView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
            <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="Delete"
                InsertMethod="Insert" SelectMethod="Select" TypeName="MonoTests.System.Web.UI.WebControls.FormViewDataObject"
                UpdateMethod="Update">
                <DeleteParameters>
                    <asp:Parameter Name="ID" Type="String" />
                    <asp:Parameter Name="FName" Type="String" />
                    <asp:Parameter Name="LName" Type="String" />
                </DeleteParameters>
                <UpdateParameters>
                    <asp:Parameter Name="ID" Type="String" />
                    <asp:Parameter Name="FName" Type="String" />
                    <asp:Parameter Name="LName" Type="String" />
                </UpdateParameters>
                <InsertParameters>
                    <asp:Parameter Name="ID" Type="String" />
                    <asp:Parameter Name="FName" Type="String" />
                    <asp:Parameter Name="LName" Type="String" />
                </InsertParameters>
            </asp:ObjectDataSource>
        </div>
    </form>
</body>
</html>
