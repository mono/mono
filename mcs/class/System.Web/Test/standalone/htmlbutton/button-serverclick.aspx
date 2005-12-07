<!-- bug 35677 -->

<html>
	<script language="C#" runat="server">
		void onclick (object sender, EventArgs e)
		{			
			c.Value = Convert.ToString (Convert.ToInt32 (a.Value) + Convert.ToInt32(b.Value));
		}
	</script>

	<body>
		<form id="Form1" method="post" runat="server">
			<input id="a" type="text" runat="server" value="1" />
			+
			<input id="b" type="text" runat="server" value="1" />
			<input OnServerClick="onclick" type="button" value="=" runat="server">
			<INPUT id="c" type="text" runat="server">
		</form>
	</body>
</html>
