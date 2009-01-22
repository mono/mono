<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" EnableEventValidation="false" Inherits="MyPage"  %>
<%@ Import Namespace="MonoTests.SystemWeb.Framework" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
    protected void Item_Clicked (object sender, MenuEventArgs e)
    {
        WebTest.CurrentTest.UserData = "MenuItemClick";
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Menu ID="Menu1" runat="server" OnMenuItemClick="Item_Clicked">
         <Items>
         <asp:MenuItem Text="root" Value="root" Selected="true" Enabled="true">
            <asp:MenuItem Text="node1" Value="node1" Enabled="true" />
            <asp:MenuItem Text="node2" Value="node2" Enabled="true" />
         </asp:MenuItem>
         </Items>
        </asp:Menu>
    </div>
    </form>
</body>
</html>
