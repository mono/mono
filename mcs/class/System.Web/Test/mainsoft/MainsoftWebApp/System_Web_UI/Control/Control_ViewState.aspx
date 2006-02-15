<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Control_ViewState.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI.Control_ViewState" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Control_ViewState</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="Visual Basic .NET 7.1" name="CODE_LANGUAGE">
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
		  theform.submit();
        }
		</script>
	</HEAD>
	<body>
		<form id="Form1" method="post" runat="server">
			<!-- Button is used for Test Post Back -->
			<asp:Button id="Button1" runat="server" Text="PostBack" BorderStyle="Groove"></asp:Button>
		</form>
	</body>
</HTML>
