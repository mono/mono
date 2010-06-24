<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><div>
        <asp:DetailsView ID="DetailsView1" runat="server" BackImageUrl="Blue_hills.jpg"
            Caption="Caption Test" CaptionAlign="Bottom" CellPadding="30" CellSpacing="20"
            DataSourceID="ObjectDataSource1" Height="50px" Width="125px">
        </asp:DetailsView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="Delete"
            InsertMethod="Insert" SelectMethod="GetMyData" TypeName="MonoTests.System.Web.UI.WebControls.TableObject"
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
    </div><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </form>
</body>
</html>
