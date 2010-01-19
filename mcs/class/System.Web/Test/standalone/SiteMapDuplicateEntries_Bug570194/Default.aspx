<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head runat="server">
	<title>Default</title>
</head>
<body>
	<form runat="server">
		<%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><asp:SiteMapPath runat="server" RenderCurrentNodeAsLink="true" /><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
	</form>
</body>
</html>
