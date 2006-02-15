<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="LinkButton_Text.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.LinkButton_Text" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>LinkButton_Text</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 106; LEFT: 16px; POSITION: absolute; TOP: 16px"
				runat="server" Height="48px" Width="120px">
				<asp:LinkButton id="LinkButton1" runat="server">    abcdefghijklm nopqrstuvw xyz    </asp:LinkButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" style="Z-INDEX: 105; LEFT: 16px; POSITION: absolute; TOP: 248px"
				runat="server" Height="48px" Width="208px">
				<asp:LinkButton id="LinkButton5" runat="server"></asp:LinkButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 184px"
				runat="server" Height="48px" Width="208px">
				<asp:LinkButton id="LinkButton4" runat="server"></asp:LinkButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 128px"
				runat="server" Height="48px" Width="208px">
				<asp:LinkButton id="LinkButton3" runat="server">`1234567890-=[]\;',./~!@#$%^&*()_+{}|:"<>?</asp:LinkButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 72px"
				runat="server" Height="48px" Width="208px">
				<asp:LinkButton id="LinkButton2" runat="server">    abcdefghijklm nopqrstuvw xyz    </asp:LinkButton>
			</cc1:GHTSubTest>
		</form>
		<br>
		<br>
	</body>
</HTML>
