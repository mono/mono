<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="ApplicationPreStartMethods._default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><div>Report:<pre runat="server" id="report" /></div><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
    </form>
</body>
</html>
