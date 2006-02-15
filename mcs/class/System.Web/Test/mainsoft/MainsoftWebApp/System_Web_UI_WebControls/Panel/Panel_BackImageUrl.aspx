<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Panel_BackImageUrl.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Panel_BackImageUrl" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Panel_BackImageUrl</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 15px"
				runat="server" Width="136px" Height="80px">
				<asp:Panel id="Panel1" runat="server" Height="72px" Width="128px" BackImageUrl="http://localhost/GHTTests/System_Web_dll/System_Web_UI_WebControls/Panel/Car.jpg">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 352px"
				runat="server" Height="80px" Width="136px">
				<asp:Panel id="Panel4" runat="server" Height="72px" Width="128px" BackImageUrl="../HyperLink/Car.jpg">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 248px"
				runat="server" Height="80px" Width="136px">
				<asp:Panel id="Panel3" runat="server" Height="72px" Width="128px" BackImageUrl="/GHTTests/System_Web_dll/System_Web_UI_WebControls/Panel/Car.jpg">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 128px"
				runat="server" Height="80px" Width="136px">
				<asp:Panel id="Panel2" runat="server" Height="72px" Width="128px" BackImageUrl="Car.jpg">Panel</asp:Panel>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
