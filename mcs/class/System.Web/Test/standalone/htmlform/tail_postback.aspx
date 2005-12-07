<%@ Page Language="C#" AutoEventWireup="True" %>

<html>
<head>
<script runat="server">
	void Page_Load (object o, EventArgs e)
	{
		DataBind ();
	}
</script>
</head>
<body>
	<p>
	Instructions: put a tail (ie ...aspx/foo) in the
	location bar. Then click the button. You will get a different
	path.
	</p>

	<form runat="server">
		<asp:button runat="server" Text="Click Me!"/>
		<br>
		PathInfo: <%# Request.PathInfo %>
	</form>
</body>
</html>
