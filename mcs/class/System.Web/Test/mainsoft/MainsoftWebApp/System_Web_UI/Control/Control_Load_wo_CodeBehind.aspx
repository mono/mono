<%@ Page Language="c#" AutoEventWireup="false" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Control_Load_wo_CodeBehind</title>
		<script language="c#" runat="server">
		private void Page_Load(object sender, System.EventArgs e)
		{
			Label1.Text = "Page_Load Event was raised";
		}
		</script>
		<meta content="Microsoft Visual Studio .NET 7.NET 7.1" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script LANGUAGE="JavaScript">
        function ScriptTest()
        {
            var theform;
		    if (window.navigator.appName.toLowerCase().indexOf("netscape") > -1) {
			    theform = document.forms["Form1"];
		    }
		    else {
			    theform = document.Form1;
		    }
        }
		</script>
	</HEAD>
	<body>
		<form id="Form1" method="post" runat="server">
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="96px" Width="144px">
				<asp:Label id="Label1" runat="server">Page_Load event was not raised</asp:Label>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
