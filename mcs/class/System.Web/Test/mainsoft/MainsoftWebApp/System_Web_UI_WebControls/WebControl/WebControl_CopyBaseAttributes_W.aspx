<%@ Page Language="c#" AutoEventWireup="false" Codebehind="WebControl_CopyBaseAttributes_W.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.WebControl_CopyBaseAttributes_W" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>WebControl_CopyBaseAttributes_W</title>
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
			<asp:button id="btnAccessKey" accessKey="Z" runat="server" Text="Button"></asp:button><asp:button id="btnEnabled" runat="server" Text="Button" Enabled="False"></asp:button><asp:button id="btnToolTip" runat="server" Text="Button" ToolTip="ToolTip text"></asp:button><asp:button id="btnTabIndex" tabIndex="5" runat="server" Text="Button"></asp:button><asp:button id="btnAttributes" runat="server" Text="Button" TestAttribute="TestAttribute Value"></asp:button></form>
	</body>
</HTML>
