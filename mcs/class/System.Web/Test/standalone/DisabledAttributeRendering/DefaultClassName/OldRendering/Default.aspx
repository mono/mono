<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><asp:Panel runat="server" Enabled="false" CssClass="MyClass">I am disabled</asp:Panel><asp:LinkButton runat="server" Enabled="false" Text="Disabled link" PostBackUrl="~/Default.aspx"/><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
    </div>
    </form>
</body>
</html>
