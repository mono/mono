<%@ Page Language="c#" AutoEventWireup="false" Codebehind="RadioButton_GroupName.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.RadioButton_GroupName" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>RadioButton_GroupName</title>
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
				runat="server" Width="136px" Height="24px">
				<asp:RadioButton id="RadioButton1" runat="server"></asp:RadioButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest7" style="Z-INDEX: 107; LEFT: 16px; POSITION: absolute; TOP: 64px"
				runat="server" Height="24px" Width="136px">
				<asp:RadioButton id="RadioButton7" runat="server" GroupName=""></asp:RadioButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" style="Z-INDEX: 106; LEFT: 16px; POSITION: absolute; TOP: 192px"
				runat="server" Height="24px" Width="136px">
				<asp:RadioButton id="RadioButton6" runat="server" GroupName="Group11"></asp:RadioButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" style="Z-INDEX: 105; LEFT: 16px; POSITION: absolute; TOP: 360px"
				runat="server" Height="24px" Width="136px">
				<asp:RadioButton id="RadioButton5" runat="server" GroupName="Group2"></asp:RadioButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 320px"
				runat="server" Height="24px" Width="136px">
				<asp:RadioButton id="RadioButton4" runat="server" GroupName="Group2"></asp:RadioButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 280px"
				runat="server" Height="24px" Width="136px">
				<asp:RadioButton id="RadioButton3" runat="server" GroupName="Group2"></asp:RadioButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 136px"
				runat="server" Height="24px" Width="136px">
				<asp:RadioButton id="RadioButton2" runat="server" GroupName="Group1"></asp:RadioButton>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
