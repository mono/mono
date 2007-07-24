<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Register Namespace="UpdatePanelTutorialCustom.CS" TagPrefix="sample" Assembly="SystemWebExtensionsAUT" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Browse Products</title>
    <script runat="server">
        protected void ProductsView1_RowCommand(object sender, EventArgs e)
        {
            ShoppingCartList.DataSource = ProductsView1.Cart;
            ShoppingCartList.DataBind();
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager runat="server" ID="ScriptManager1" EnablePartialRendering="false" />
        <h2>Browse Products</h2>
        <sample:ProductsView runat="server" ID="ProductsView1" PageSize="5" OnRowCommand="ProductsView1_RowCommand" />
                                 
        <asp:UpdatePanel runat="server" ID="CartUpdatePanel">
          <ContentTemplate>
            <h3>Selected Items</h3>
            <asp:BulletedList BulletStyle="numbered" runat="server" ID="ShoppingCartList" />
          </ContentTemplate>
        </asp:UpdatePanel>
     </div>
    </form>
</body>
</html>
