<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="ListBox_ToolTip.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.ListBox_ToolTip" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>ListBox_ToolTip</title>
		<META http-equiv="Content-Type" content="text/html; charset=windows-1252">
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
			<cc1:ghtsubtest id="GHTSubTest1" runat="server" Height="96px" Width="416px">
				<asp:ListBox id="ListBox1" runat="server"></asp:ListBox>
			</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest2" runat="server" Height="96px" Width="416px">
				<asp:ListBox id="ListBox2" runat="server"></asp:ListBox>
			</cc1:ghtsubtest>
			<br>
			<cc1:ghtsubtest id="GHTSubTest3" runat="server" Height="96px" Width="416px">
				<asp:ListBox id="ListBox3" runat="server"></asp:ListBox>
			</cc1:ghtsubtest>
		</form>
	</body>
</HTML>
