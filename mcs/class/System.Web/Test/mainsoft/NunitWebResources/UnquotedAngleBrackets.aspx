<%@ Page Language="C#" %>
<html>
<head><title>unquoted angle brackets</title></head>
<body>
<form runat="server">
<asp:DropDownList runat="server">
<asp:ListItem Value="&gt;"> > </asp:ListItem>
<asp:ListItem Value="="> = </asp:ListItem>
<asp:ListItem Value="&lt;"> < </asp:ListItem>
</asp:DropDownList>
</form>
</body>
</html>
