<%@ Page Language="C#" AutoEventWireup="true" CodeFile="default.aspx.cs" Inherits="_default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:SiteMapDataSource ID="SiteMapDataSource1" runat="server" ShowStartingNode="False" StartingNodeUrl="~/" />
    <asp:Menu ID="NavigationMenu" runat="server" DataSourceID="SiteMapDataSource1"/>
    <div><%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><pre runat="server" id="log" /><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %></div>
    </form>
</body>
</html>
