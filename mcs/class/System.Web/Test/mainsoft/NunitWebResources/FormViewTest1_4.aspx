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
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:FormView ID="FormView4" runat="server" AllowPaging="True" BackColor="White"
            BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" CellSpacing="10"
            DataSourceID="ObjectDataSource1" DefaultMode="Edit" FooterText="Using Footer Text property"
            GridLines="Both" HeaderText="Using Header Text property" HorizontalAlign="Right" CaptionAlign="Right">
            <FooterStyle BackColor="Maroon" ForeColor="#000066" HorizontalAlign="Center" />
            <EditRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
            <PagerTemplate>
                <asp:LinkButton ID="LinkButton1" runat="server" CommandArgument='<%# "Next" %>' Text='<%# "Next" %>' CommandName='<%# "Page" %>'></asp:LinkButton>
                <asp:LinkButton ID="LinkButton2" runat="server" CommandArgument='<%# "Prev" %>' Text='<%# "Prev" %>' CommandName='<%# "Page" %>'></asp:LinkButton>
                <asp:Label ID="Label7" runat="server" Text='<%# "Page Index: "+ FormView4.PageIndex %>'></asp:Label>
            </PagerTemplate>
            <RowStyle BackColor="#FF8080" ForeColor="#000066" HorizontalAlign="Center" />
            <PagerStyle BackColor="LightGray" ForeColor="#000066" HorizontalAlign="Left" />            
            <ItemTemplate>
                &nbsp;<asp:TextBox ID="TextBox1" runat="server" Text="<%# FormView4.DataItem.ToString() %>"></asp:TextBox>
            </ItemTemplate>
            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" HorizontalAlign="Left" />
        </asp:FormView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </form>
</body>
</html>
endtest