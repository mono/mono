<%@ Page Language="C#" %>
<script runat="server">
	void Page_Load (object sender, EventArgs e)
	{
		throw new InvalidOperationException ("test");
	}
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head runat="server">
	<title>Default</title>
</head>
<body>
	<form id="form1" runat="server">
	</form>
</body>
</html>