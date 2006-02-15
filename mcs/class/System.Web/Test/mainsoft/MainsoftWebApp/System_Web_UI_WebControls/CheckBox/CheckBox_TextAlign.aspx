<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="CheckBox_TextAlign.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.CheckBox_TextAlign" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>CheckBox_TextAlign</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 15px"
				runat="server" Width="160px" Height="32px">
				<asp:CheckBox id="CheckBox1" runat="server"></asp:CheckBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 104px"
				runat="server" Height="32px" Width="160px">
				<asp:CheckBox id="CheckBox3" runat="server" TextAlign="Right"></asp:CheckBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 56px"
				runat="server" Height="32px" Width="160px">
				<asp:CheckBox id="CheckBox2" runat="server" TextAlign="Left"></asp:CheckBox>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
