<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HttpSessionState_IsNewSession2.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_SessionState.HttpSessionState_IsNewSession2" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HttpSessionState_IsNewSession2</title>
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
	</HEAD>
	<body>
		<FORM id="Form1" method="post" runat="server">
			By pressing "<STRONG>browser refresh</STRONG>" button, session state should 
			leave <STRONG>true.<BR>
			</STRONG>Because Session object was untouched and therefore was not saved.</FORM>
	</body>
</HTML>
