<!-- bug 47725 -->
<%@ Page language="C#" %>
<html>
	<head>
		<script runat="server">
		void c (object sender, EventArgs e)
		{
			b.Font.Bold = !b.Font.Bold;
		}
		</script>
	</head>
	<body>
		Clicking on this button should toggle its boldness.
		<br />
		<form runat="server">
			<asp:button id="b" runat="server" Text="Test" OnClick="c" />
		</form>
	</body>
</html>
