<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="ValidationPropertyAttribute_ctor_S.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI.ValidationPropertyAttribute_ctor_S" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>ValidationPropertyAttribute_ctor_S</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="Visual Basic .NET 7.1" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script language="JavaScript">
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
	<body MS_POSITIONING="GridLayout">
		<form id="Form1" method="post" runat="server">
			&nbsp;
			<asp:RegularExpressionValidator id="RegularExpressionValidator1" style="Z-INDEX: 101; LEFT: 104px; POSITION: absolute; TOP: 136px"
				runat="server" ErrorMessage="RegularExpressionValidator"></asp:RegularExpressionValidator></form>
	</body>
</HTML>
