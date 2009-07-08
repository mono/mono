<%@ Page Language="C#" %>
<html><head><title>Bug 520024</title></head>
<body>
<form runat="server">
<% #if DEBUG %>
	Debug mode
<% #else %>
	Normal mode
<% #endif %>
</form>
</body>
</html>
