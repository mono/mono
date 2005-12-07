<!-- bug 51582 -->
<%@ Page language="c#" %>
<html>
	<body>
		<form runat="server">
			<input id="Radio1" type="radio" value="Radio1" name="RadioGroup" runat="server">
			<input id="Radio2" type="radio" value="Radio2" name="Radio2" runat="server">
			<input id="Radio3" type="radio" value="Radio3" name="Radio3" runat="server">
			<asp:button runat="server" text="submit" />
			<asp:label id="t" runat="server" />
		</form>
	</body>
	<script language="C#" runat="server">
		void Page_Load (object sender, EventArgs e)
		{
			if (Radio1.Checked == true)
				t.Text = "Radio1 is checked";
			else if (Radio2.Checked == true)
				t.Text = "Radio2 is checked";
			else if (Radio3.Checked == true)
				t.Text = "Radio3 is checked";
		}
	</script>
</html>
