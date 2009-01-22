<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

	protected void Button1_Click (object sender, EventArgs e) {
		TextBox1.Text = "Button1_Click";
	}

	protected void LinkButton1_Click (object sender, EventArgs e) {
		TextBox1.Text = "LinkButton1_Click";
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
		<asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
		<br />
		<asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Button" />
		<br />
		<asp:LinkButton ID="LinkButton1" runat="server" OnClick="LinkButton1_Click">LinkButton</asp:LinkButton></div>
    </form>
</body>
</html>
