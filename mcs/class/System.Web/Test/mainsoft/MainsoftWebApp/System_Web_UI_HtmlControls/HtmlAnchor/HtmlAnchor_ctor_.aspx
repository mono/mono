<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HtmlAnchor_ctor_.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_HtmlControls.HtmlAnchor_ctor_" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HtmlAnchor_ctor_</title>
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
	<body>
		<form id="Form1" method="post" runat="server">
			<P><cc1:ghtsubtest id="GHTSubTest1" runat="server" Height="64px" Width="553px" Description="HTMLAnchor_ctor_1"><A id="anchor1" href="telnet://192.1.1.1" name="anchor1" runat="server">Anchor1</A>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="GHTSubTest2" runat="server" Height="64px" Width="553px" Description="HTMLAnchor_ctor_2"><A id="anchor2" title="title" href="http://localhost/" target="_blank" name="anchor2"
						runat="server">Anchor2</A>
				</cc1:ghtsubtest></P>
		</form>
		<br>
		<br>
	</body>
</HTML>
