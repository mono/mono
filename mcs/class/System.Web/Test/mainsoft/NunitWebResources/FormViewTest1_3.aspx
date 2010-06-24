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
    </div>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:FormView ID="FormView3" runat="server" AllowPaging="True" BackColor="LightGoldenrodYellow"
            BorderColor="Tan" BorderWidth="1px" CellPadding="2" DataSourceID="ObjectDataSource1"
            ForeColor="Black">
            <FooterStyle BackColor="Tan" ForeColor="#FFC0FF" HorizontalAlign="Right" />
            <EditRowStyle BackColor="DarkSlateBlue" ForeColor="GhostWhite" />
            <PagerStyle BackColor="PaleGoldenrod" ForeColor="DarkSlateBlue" HorizontalAlign="Center" />
            <FooterTemplate>
                <asp:Label ID="Label6" runat="server" Text='<%# "FormView Footer" %>'></asp:Label>
            </FooterTemplate>
            <ItemTemplate>
                <asp:Label ID="Label4" runat="server" Text="<%# FormView3.DataItem.ToString() %>"></asp:Label>
            </ItemTemplate>
            <HeaderStyle BackColor="Tan" Font-Bold="True" ForeColor="#C00000" HorizontalAlign="Center" VerticalAlign="Top" />
            <HeaderTemplate>
                <asp:Label ID="Label5" runat="server" Text='<%# "Header Template Test" %>'></asp:Label>
            </HeaderTemplate>
        </asp:FormView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </form>
</body>
</html>
endtest