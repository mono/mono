<html>
<head>
<script language="C#" runat=server>
	void Click(object s, EventArgs e)
	{
        	if (Page.IsValid == true)
			lblOutput.Text = "PASS";
		else
			lblOutput.Text = "FAIL";
	}
</script>
</head>
<body>

<form runat="server">

<p>
Just click validate. It should say pass.
</p>

<asp:label ID="lblOutput" runat=server /><br>

<asp:textbox id="t1" runat=server />
<asp:requiredfieldvalidator id="r1" ControlToValidate="t1" Enabled="false" runat="server" />

<asp:button id="b1" text="Validate" OnClick="Click" runat="server" />
</form>
</body>
</html>
