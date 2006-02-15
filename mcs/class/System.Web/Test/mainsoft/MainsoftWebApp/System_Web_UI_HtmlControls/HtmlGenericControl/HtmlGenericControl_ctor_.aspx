<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HtmlGenericControl_ctor_.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_HtmlControls.HtmlGenericControl_ctor_" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HtmlGenericControl_ctor_</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="553px" Height="88px" Description="HtmlGenericControl_ctor_1">
				<DIV id="myDiv1" runat="server">text in div</DIV>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="88px" Width="553px" Description="HtmlGenericControl_ctor_2">
				<SPAN id="mySpan1" runat="server">text in span</SPAN>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="96px" Width="553px" Description="HtmlGenericControl_ctor_3">
				<P id="myParagraph1" runat="server">text in paragraph1</P>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="96px" Width="553px" Description="HtmlGenericControl_ctor_4">
				<P id="myParagraph2" runat="server">text in paragraph2</P>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
