<%@ Page Language="C#" Debug="true" %>
<html>
<body>
Actual: "<%=anchor1.HRef%>", Expected "~/otherfile.txt" (not resolved)<br>
<a id="anchor1" runat="server" href="~/otherfile.txt">This link should point at <%= ResolveUrl ("~/otherfile.txt") %></a>
<br>Actual: "<%=anchor1.HRef%>", Expected "" (empty)<br>
</body>
</html>

