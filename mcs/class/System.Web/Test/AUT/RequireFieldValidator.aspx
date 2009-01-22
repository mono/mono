<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

	protected void Page_Load (object sender, EventArgs e) {
		TextBox0.Text = DateTime.Now.Ticks.ToString ();
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
		<asp:TextBox ID="TextBox0" runat="server" Text="Label"></asp:TextBox>
		<br />
		<asp:TextBox ID="TextBox1" runat="server"></asp:TextBox><asp:RequiredFieldValidator
			ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBox1" ErrorMessage="RequiredFieldValidator1"></asp:RequiredFieldValidator><br />
		<asp:Button ID="Button1" runat="server" Text="Button1" />&nbsp;
		<br />
		<asp:TextBox ID="TextBox2" runat="server"></asp:TextBox><asp:RequiredFieldValidator
			ID="RequiredFieldValidator2" runat="server" ControlToValidate="TextBox2" ErrorMessage="RequiredFieldValidator2"
			ValidationGroup="vg"></asp:RequiredFieldValidator>
		<br />
		<asp:Button ID="Button2" runat="server" Text="Button2" ValidationGroup="vg" /></div>
    </form>
</body>
</html>
