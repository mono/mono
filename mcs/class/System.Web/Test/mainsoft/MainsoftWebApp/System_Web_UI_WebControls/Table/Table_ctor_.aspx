<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Table_ctor_.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Table_ctor_" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Table_ctor_</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 100; LEFT: 16px; POSITION: absolute; TOP: 15px"
				runat="server" Width="553px" Height="296px">
				<asp:Table id="Table1" runat="server" Height="48px" Width="128px"></asp:Table>
				<asp:Table id="Table2" runat="server" Height="120px" Width="312px" CaptionAlign="Bottom" Caption="Caption Text"></asp:Table>
			</cc1:GHTSubTest>
		</form>
		<br>
		<br>
	</body>
</HTML>
