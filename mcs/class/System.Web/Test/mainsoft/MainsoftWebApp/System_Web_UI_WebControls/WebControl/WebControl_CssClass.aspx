<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="WebControl_CssClass.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.WebControl_CssClass" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>WebControl_CssClass</title>
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
        }
		</script>
		<!-- Styles to use in the test -->
		<style>
		.CssStyle1 { FONT-SIZE: large; Font-Bold: True; Font-Italic: True; ForeColor: orange; BackColor: blue; Alignment: Right; Wrapping: Wrap }
		.CssStyle2 { FONT-SIZE: small; Font-Bold: false; Font-Italic: false; ForeColor: darkgreen; BackColor: red; Alignment: center; Wrapping: no-wrap }
		</style>
	</HEAD>
	<body>
		<form id="Form1" method="post" runat="server">
			<P>&nbsp;</P>
			<P>&nbsp;</P>
		</form>
	</body>
</HTML>
