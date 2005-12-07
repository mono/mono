<!-- bug 49091 -->
<%@ Page language="c#" %>
<html>
	<head>
		<script runat="server">		
		private void BtnDisable_Click(object sender, System.EventArgs e)
		{
			CheckBox1.Enabled = false;
		}
		
		private void BtnEnable_Click(object sender, System.EventArgs e)
		{
			CheckBox1.Enabled = true;
		}
		</script>
	</head>
	<body>
		<p>Check the checkbox, click disable then enable. The checkbox should stay enabled.</p>
		<form runat="server">
			<asp:checkbox id="CheckBox1" runat="server" Text="Test" />
			<asp:button onclick="BtnDisable_Click" runat="server" Text="Disable" />
			<asp:button onclick="BtnEnable_Click" runat="server" Text="Enable" />
		</form>
	</body>
</html>
