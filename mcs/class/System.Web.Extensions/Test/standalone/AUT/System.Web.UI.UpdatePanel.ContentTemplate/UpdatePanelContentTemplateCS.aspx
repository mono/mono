
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
	"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

	protected void Button1_Click(object sender, EventArgs e)
	{
		// Access the Label1 control in the ContentTemplate.
		Label1.Text = "Panel refreshed at " + DateTime.Now.ToString();
	}

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>UpdatePanel Example</title>
</head>
<body>
	<form id="form1" runat="server">
		<div>
			<asp:ScriptManager ID="ScriptManager1"
			                   runat="server" />
			<asp:UpdatePanel ID="UpdatePanel1" 
			                 UpdateMode="Conditional" 
			                 runat="server">
				<ContentTemplate>
					<asp:Label ID="Label1" 
					           runat="server">A full page postback occurred.
					</asp:Label>
					<br />
					<asp:Button ID="Button1"
					            Text="Refresh Panel"
					            OnClick="Button1_Click"
					            runat="server"  />
				</ContentTemplate>
			</asp:UpdatePanel>
			<br />
		</div>
	</form>
</body>
</html>
