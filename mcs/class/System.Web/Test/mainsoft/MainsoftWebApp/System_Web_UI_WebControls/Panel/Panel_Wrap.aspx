<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Panel_Wrap.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Panel_Wrap" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Panel_Wrap</title>
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
		<form id="Form1" method="post" runat="server">
			<P>
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="160px" Height="96px">
					<asp:Panel id="Panel1" runat="server" Height="56px" Width="112px">Panel</asp:Panel>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="96px" Width="160px">
					<asp:Panel id="Panel2" runat="server" Height="56px" Width="112px" Wrap="False">Panel</asp:Panel>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="96px" Width="160px">
					<asp:Panel id="Panel3" runat="server" Height="56px" Width="112px" Wrap="True">Panel</asp:Panel>
				</cc1:GHTSubTest></P>
		</form>
		<br>
		<br>
	</body>
</HTML>
