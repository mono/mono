<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Table_GridLines.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Table_GridLines" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Table_GridLines</title>
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
		</form>
		<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="144px" Height="48px">
			<asp:Table id="Table1" runat="server" GridLines="Horizontal"></asp:Table>
		</cc1:GHTSubTest>
		<br>
		<br>
		<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="40px" Width="136px">
			<asp:Table id="Table2" runat="server" GridLines="Vertical"></asp:Table>
		</cc1:GHTSubTest>
		<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="40px" Width="136px">
			<asp:Table id="Table3" runat="server" GridLines="Both"></asp:Table>
		</cc1:GHTSubTest>
		<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="40px" Width="144px">
			<asp:Table id="Table4" runat="server"></asp:Table>
		</cc1:GHTSubTest>
	</body>
</HTML>
