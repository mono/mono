<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Panel_HorizontalAlign.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Panel_HorizontalAlign" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Panel_HorizontalAlign</title>
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
	<body MS_POSITIONING="GridLayout">
		<form id="Form1" method="post" runat="server">
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 100; LEFT: 16px; POSITION: absolute; TOP: 15px"
				runat="server" Width="136px" Height="40px">
				<asp:Panel id="Panel1" runat="server" Height="48px" Width="112px">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" style="Z-INDEX: 106; LEFT: 16px; POSITION: absolute; TOP: 416px"
				runat="server" Height="40px" Width="136px">
				<asp:Panel id="Panel6" runat="server" Height="48px" Width="112px" HorizontalAlign="NotSet">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" style="Z-INDEX: 105; LEFT: 16px; POSITION: absolute; TOP: 336px"
				runat="server" Height="40px" Width="136px">
				<asp:Panel id="Panel5" runat="server" Height="48px" Width="112px" HorizontalAlign="Justify">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 256px"
				runat="server" Height="40px" Width="136px">
				<asp:Panel id="Panel4" runat="server" Height="48px" Width="112px" HorizontalAlign="Right">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 176px"
				runat="server" Height="40px" Width="136px">
				<asp:Panel id="Panel3" runat="server" Height="48px" Width="112px" HorizontalAlign="Center">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 96px"
				runat="server" Height="40px" Width="136px">
				<asp:Panel id="Panel2" runat="server" Height="48px" Width="112px" HorizontalAlign="Left">Panel</asp:Panel>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
