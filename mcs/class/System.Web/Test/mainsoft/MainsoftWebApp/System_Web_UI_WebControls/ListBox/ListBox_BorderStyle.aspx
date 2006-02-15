<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="ListBox_BorderStyle.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.ListBox_BorderStyle" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>ListBox_BorderStyle</title>
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
			<cc1:GHTSubTest id="GHTSubTest9" style="Z-INDEX: 109; LEFT: 16px; POSITION: absolute; TOP: 320px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox9" runat="server" Width="80px" Height="40px" BorderStyle="Dotted"></asp:ListBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest8" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 192px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox8" runat="server" Width="80px" Height="40px" BorderStyle="Double"></asp:ListBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest7" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 128px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox7" runat="server" Width="80px" Height="40px" BorderStyle="Groove"></asp:ListBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 64px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox6" runat="server" Width="80px" Height="40px" BorderStyle="Inset"></asp:ListBox>
			</cc1:GHTSubTest>
			<cc1:ghtsubtest id="GHTSubTest6" style="Z-INDEX: 108; LEFT: 16px; POSITION: absolute; TOP: 8px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox1" runat="server" Width="80px" Height="40px" BorderStyle="Dashed"></asp:ListBox>
			</cc1:ghtsubtest>
			<cc1:GHTSubTest id="GHTSubTest10" style="Z-INDEX: 110; LEFT: 16px; POSITION: absolute; TOP: 376px"
				runat="server" Width="88px" Height="48px">
				<asp:ListBox id="ListBox10" runat="server" Width="80px" Height="40px" BorderStyle="Solid"></asp:ListBox>
			</cc1:GHTSubTest><cc1:ghtsubtest id="GHTSubTest5" style="Z-INDEX: 106; LEFT: 16px; POSITION: absolute; TOP: 256px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox5" runat="server" Width="80px" Height="40px" BorderStyle="None"></asp:ListBox>
			</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest4" style="Z-INDEX: 105; LEFT: 16px; POSITION: absolute; TOP: 192px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox4" runat="server" Width="80px" Height="40px" BorderStyle="NotSet"></asp:ListBox>
			</cc1:ghtsubtest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 128px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox3" runat="server" Width="80px" Height="40px" BorderStyle="Outset"></asp:ListBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 100; LEFT: 16px; POSITION: absolute; TOP: 64px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox2" runat="server" Width="80px" Height="40px" BorderStyle="Ridge"></asp:ListBox>
			</cc1:GHTSubTest>
			<br>
			<br>
		</form>
	</body>
</HTML>
