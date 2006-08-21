<%@ Page Language="C#" AutoEventWireup="true" Codebehind="MyPage.aspx.cs" Inherits="MyPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Menu ID="Menu1" runat="server">
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
