<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HtmlAnchor_Target.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_HtmlControls.HtmlAnchor_Target" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HtmlAnchor_Target</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="553px" Height="64px" Description="HTMLAnchor_Target_1">
					<A id="A1" href="http://www.microsoft.com" target="" name="anchor1" runat="server">anchor1</A>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="553px" Height="64px" Description="HTMLAnchor_Target_2">
					<A id="A2" href="http://www.microsoft.com" target="_blank" name="anchor2" runat="server">
						anchor2</A>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Width="553px" Height="64px" Description="HTMLAnchor_Target_3">
					<A id="A3" href="http://www.microsoft.com" target="_blank" name="anchor3" runat="server">
						anchor3</A>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Width="553px" Height="64px" Description="HTMLAnchor_Target_4">
					<A id="A4" href="http://www.microsoft.com" target="_blank" name="anchor4" runat="server">
						anchor4</A>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest5" runat="server" Width="553px" Height="64px" Description="HTMLAnchor_Target_5">
					<A id="A5" href="http://www.microsoft.com" target="_blank" name="anchor5" runat="server">
						anchor5</A>
				</cc1:GHTSubTest></P>
		</form>
		<br>
		<br>
	</body>
</HTML>
