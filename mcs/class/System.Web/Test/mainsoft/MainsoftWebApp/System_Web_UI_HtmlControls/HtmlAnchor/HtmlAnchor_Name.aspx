<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HtmlAnchor_Name.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_HtmlControls.HtmlAnchor_Name" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HtmlAnchor_Name</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="553px" Height="56px" Description="HTMLAnchor_Name_1">
					<A id="anchor1" href="http://www.example.com" name="anchor1" runat="server">Anchor1</A>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="Ghtsubtest2" runat="server" Width="553px" Height="56px" Description="HTMLAnchor_Name_2">
					<A id="anchor2" href="http://www.example.com" name="anchor2_!@#$%^&amp;*()" runat="server">
						Anchor2</A>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="Ghtsubtest3" runat="server" Width="553px" Height="59px" Description="HTMLAnchor_Name_3">
					<A id="anchor3" href="http://www.example.com" name="anchor3" runat="server">Anchor3</A>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="Ghtsubtest4" runat="server" Width="553px" Height="59px" Description="HTMLAnchor_Name_4">
					<A id="anchor4" href="http://www.example.com" name="anchor4" runat="server">Anchor4</A>
				</cc1:GHTSubTest></P>
		</form>
		<br>
		<br>
	</body>
</HTML>
