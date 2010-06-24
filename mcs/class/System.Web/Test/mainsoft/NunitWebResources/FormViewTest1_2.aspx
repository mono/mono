<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server"
            DeleteMethod="DeleteList" InsertMethod="InsertList" SelectMethod="GetMyList"
            TypeName="MonoTests.System.Web.UI.WebControls.TestMyData" UpdateMethod="UpdateList">
            <DeleteParameters>
                <asp:Parameter Name="value" Type="Int32" />
            </DeleteParameters>
            <InsertParameters>
                <asp:Parameter Name="value" Type="Int32" />
            </InsertParameters>
            <UpdateParameters>
                <asp:Parameter Name="index" Type="Int32" />
                <asp:Parameter Name="value" Type="Int32" />
            </UpdateParameters>
        </asp:ObjectDataSource>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:FormView ID="FormView2" runat="server" AllowPaging="True" CellPadding="4" DataSourceID="ObjectDataSource1"
            ForeColor="#333333">
            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <EditRowStyle BackColor="#999999" />
            <PagerTemplate>
                <asp:Button ID="Button1" runat="server" CommandArgument='<%# "Prev" %>' Text='<%# "Prev Item" %>' CommandName='<%# "Page" %>' />
                <asp:Button ID="Button2" runat="server" CommandArgument='<%# "Next" %>' Text='<%# "Next Item" %>' CommandName='<%# "Page" %>' />
                <asp:Button ID="Button3" runat="server" CommandArgument='<%# "First" %>' Text='<%# "First Item" %>' CommandName='<%# "Page" %>' />
                <asp:Button ID="Button4" runat="server" CommandArgument='<%# "Last" %>' Text='<%# "Last Item" %>' CommandName='<%# "Page" %>' />
            </PagerTemplate>
            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <FooterTemplate>
                <asp:Label ID="Label3" runat="server" Text='<%# "Footer Template Test" %>'></asp:Label>
            </FooterTemplate>
            <ItemTemplate>
                <asp:Label ID="Label2" runat="server" Text="<%# FormView2.DataItem.ToString() %>"></asp:Label>
            </ItemTemplate>
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        </asp:FormView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </div>
    </form>
</body>
</html>
endtest