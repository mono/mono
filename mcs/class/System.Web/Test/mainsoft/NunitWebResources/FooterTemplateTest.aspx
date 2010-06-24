<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:DetailsView ID="DetailsView1" runat="server" AllowPaging="True" DataSourceID="ObjectDataSource1"
            Height="50px" Width="125px">
            <FooterTemplate>
                Footer Template Test<asp:HyperLink ID="HyperLink1" runat="server" Text='<%# "Footer" %>'></asp:HyperLink>
            </FooterTemplate>
        </asp:DetailsView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="CreateDataTable"
            TypeName="MonoTests.System.Web.UI.WebControls.TableObject"></asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
