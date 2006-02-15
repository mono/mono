<%@ Page Language="c#" AutoEventWireup="false" Codebehind="CheckBox_Text.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.CheckBox_Text" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>CheckBox_Text</title>
		<meta content="Microsoft Visual Studio .1" name="GENERATOR">
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
				runat="server" Width="232px" Height="48px">
				<asp:CheckBox id="CheckBox1" runat="server" Text="  abcdefghijklmnopq  rstuvwxyz  "></asp:CheckBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" style="Z-INDEX: 106; LEFT: 16px; POSITION: absolute; TOP: 312px"
				runat="server" Height="48px" Width="176px">
				<asp:CheckBox id="CheckBox6" runat="server" Text="`~!@#$%^&amp;*()_+-={}|[]\;':&quot;,./??<>"></asp:CheckBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" style="Z-INDEX: 105; LEFT: 16px; POSITION: absolute; TOP: 248px"
				runat="server" Height="48px" Width="176px">
				<asp:CheckBox id="CheckBox5" runat="server" Text=""></asp:CheckBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 184px"
				runat="server" Height="48px" Width="152px">
				<asp:CheckBox id="CheckBox4" runat="server" Width="120px" Text="aaaaaaaaaa aaaaaaaaaaa"></asp:CheckBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 128px"
				runat="server" Height="48px" Width="176px">
				<asp:CheckBox id="CheckBox3" runat="server" Text="               "></asp:CheckBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 72px"
				runat="server" Height="48px" Width="176px">
				<asp:CheckBox id="CheckBox2" runat="server" Text="1234567890"></asp:CheckBox>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
