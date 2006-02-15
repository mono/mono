<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="ListBox_BorderColor.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.ListBox_BorderColor" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>ListBox_BorderColor</title>
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
			<cc1:ghtsubtest id="GHTSubTest1" style="Z-INDEX: 100; LEFT: 16px; POSITION: absolute; TOP: 16px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox1" runat="server" Width="80px" Height="40px" BorderColor="#33ffcc"></asp:ListBox>
			</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest5" style="Z-INDEX: 105; LEFT: 16px; POSITION: absolute; TOP: 272px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox5" runat="server" Width="80px" Height="40px" BorderColor="Yellow"></asp:ListBox>
			</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest4" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 208px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox4" runat="server" Width="80px" Height="40px" BorderColor="CornflowerBlue"></asp:ListBox>
			</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest3" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 144px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox3" runat="server" Width="80px" Height="40px" BorderColor="#93a070"></asp:ListBox>
			</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest2" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 80px"
				runat="server" Height="48px" Width="88px">
				<asp:ListBox id="ListBox2" runat="server" Width="80px" Height="40px" BorderColor="#9d1173"></asp:ListBox>
			</cc1:ghtsubtest></form>
		<br>
		<br>
	</body>
</HTML>
