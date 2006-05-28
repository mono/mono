<%@ Page Language="C#" StylesheetTheme="Theme1" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
 
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
            <asp:Label ID="Label" runat="server" Text="Label" />
            <asp:Label ID="LabelOverride" runat="server" Text="Label" BackColor="White" />
            <asp:Label ID="LabelRed" SkinID="red" runat="server" Text="LabeRed" />
            <asp:Label ID="LabelYellow" SkinID="yellow" runat="server" Text="Label" />
            
            <asp:Image ID="Image" runat="server" />
            <asp:Image ID="ImageOverride" runat="server" ImageUrl="overridedurl" />
            <asp:Image ID="ImageRed" SkinID="red" runat="server" />
            <asp:Image ID="ImageYellow" SkinID="yellow" runat="server" />
    </div>
    </form>
</body>
</html>
