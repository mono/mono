
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
	"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

	protected void Page_Load(object sender, EventArgs e)
	{
		UpdatePanel up1 = new UpdatePanel();
		up1.ID = "UpdatePanel1";
		up1.UpdateMode = UpdatePanelUpdateMode.Conditional;
		// The CustomContentTemplate class defines the contents of the UpdatePanel.
		ITemplate t = new CustomContentTemplate();
		up1.ContentTemplate = t;
		Panel1.Controls.Add(up1);

	}

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>UpdatePanel ContentTemplate Example</title>
</head>
<body>
	<form id="form1" runat="server">
		<div>
			<asp:ScriptManager ID="ScriptManager1" 
			                   runat="server" />
			<asp:Panel ID="Panel1" 
			           GroupingText="Programmatically Added UpdatePanel"
			           runat="server" >
			</asp:Panel>
			<br />
		</div>
	</form>
</body>
</html>
