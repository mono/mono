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
        MonoTests.System.Web.UI.WebControls.TableObject.ds = MonoTests.System.Web.UI.WebControls.TableObject.CreateDataTable (); 
    }
</script>
    <form id="form1" runat="server">
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="Delete"
            SelectMethod="GetMyData" TypeName="MonoTests.System.Web.UI.WebControls.TableObject">
            <DeleteParameters>
                <asp:Parameter Name="ID" Type="String" />
                <asp:Parameter Name="FName" Type="String" />
                <asp:Parameter Name="LName" Type="String" />
            </DeleteParameters>
        </asp:ObjectDataSource>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:DetailsView ID="DetailsView3" runat="server" AllowPaging="True" DataSourceID="ObjectDataSource1"
            Height="50px" Width="125px">
            <EmptyDataTemplate>
                <asp:TextBox ID="TextBox1" runat="server" Text='<%# "Empty Data Template" %>'></asp:TextBox>
            </EmptyDataTemplate>
            <Fields>
                <asp:ButtonField Text="TestButtonField" />
                <asp:HyperLinkField FooterText="Footer field" />
                <asp:ImageField HeaderText="Image field">
                </asp:ImageField>
                <asp:CheckBoxField Text="CheckBoxField" />
                <asp:TemplateField HeaderText="Template Field">
                    <ItemStyle BackColor="#FFE0C0" />
                </asp:TemplateField>
            </Fields>
        </asp:DetailsView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </form>
</body>
</html>
