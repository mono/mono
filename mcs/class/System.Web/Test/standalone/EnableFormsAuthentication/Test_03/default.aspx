<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test 03</title>
</head>
<body>
    <form id="form1" runat="server">
    <%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><div>Default URL: <%= FormsAuthentication.DefaultUrl %><br />Login URL: <%= FormsAuthentication.LoginUrl %></div><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
    </form>
</body>
</html>
