<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
<script runat="server">

    protected void Page_Load (object sender, EventArgs e)
    {
        DetailsView1.DataKeyNames = new string[] { "ID" };
        MonoTests.System.Web.UI.WebControls.TableObject.ds = MonoTests.System.Web.UI.WebControls.TableObject.CreateDataTable();
        
        
        
    }
    
</script>
    <form id="form1" runat="server">
    <div>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:DetailsView ID="DetailsView1" runat="server" AllowPaging="True" DataSourceID="ObjectDataSource1"
            Height="50px" Width="125px"  >
            <Fields>
                <asp:CommandField ShowDeleteButton="True" ShowInsertButton="True" ShowEditButton="True" />
            </Fields>
        </asp:DetailsView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="Delete"
            SelectMethod="GetMyData" TypeName="MonoTests.System.Web.UI.WebControls.TableObject" InsertMethod="Insert" UpdateMethod="Update" >
            <DeleteParameters>
                <asp:Parameter Name="ID" Type="String" />
                <asp:Parameter Name="FName" Type="String" />
                <asp:Parameter Name="LName" Type="String" />
            </DeleteParameters>
            <InsertParameters>
                <asp:Parameter Name="ID" Type="String" />
                <asp:Parameter Name="FName" Type="String" />
                <asp:Parameter Name="LName" Type="String" />
            </InsertParameters>
            <UpdateParameters>
                <asp:Parameter Name="ID" Type="String" />
                <asp:Parameter Name="FName" Type="String" />
                <asp:Parameter Name="LName" Type="String" />
            </UpdateParameters>
        </asp:ObjectDataSource> 
    </div>
    </form>
</body>
</html>
