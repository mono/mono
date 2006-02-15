<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HtmlTextArea_Value.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_HtmlControls.HtmlTextArea_Value" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HtmlTextArea_Value</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="553px" Height="56px" Description="HtmlTextArea_Value_1">
					<TEXTAREA id="TEXTAREA1" name="TEXTAREA1" rows="2" cols="20" runat="server">					</TEXTAREA>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="56px" Width="553px" Description="HtmlTextArea_Value_2">
					<TEXTAREA id="TEXTAREA2" name="TEXTAREA2" rows="2" cols="20" runat="server">Sample Value</TEXTAREA></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="56px" Width="553px" Description="HtmlTextArea_Value_3">
					<TEXTAREA id="TEXTAREA3" name="TEXTAREA3" rows="2" cols="20" runat="server">123</TEXTAREA></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="56px" Width="553px" Description="HtmlTextArea_Value_4">
					<TEXTAREA id="TEXTAREA4" name="TEXTAREA4" rows="2" cols="20" runat="server">"text text text"</TEXTAREA></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest5" runat="server" Height="56px" Width="553px" Description="HtmlTextArea_Value_5">
					<TEXTAREA id="TEXTAREA5" name="TEXTAREA5" rows="2" cols="20" runat="server"></TEXTAREA></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest6" runat="server" Height="56px" Width="553px" Description="HtmlTextArea_Value_6">
					<TEXTAREA id="TEXTAREA6" name="TEXTAREA6" rows="2" cols="20" runat="server"></TEXTAREA></cc1:GHTSubTest>&nbsp;</P>
		</form>
	</body>
</HTML>
