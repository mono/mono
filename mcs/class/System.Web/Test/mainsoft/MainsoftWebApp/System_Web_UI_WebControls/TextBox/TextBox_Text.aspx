<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TextBox_Text.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TextBox_Text" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TextBox_Text</title>
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
			<P>
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="288px" Height="32px">
					<asp:TextBox id="TextBox1" runat="server" Height="24px" Width="280px">  abcdefgh ijklmnopqr  stuvwxyz    </asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="32px" Width="288px">
					<asp:TextBox id="TextBox2" runat="server" Height="24px" Width="280px">1234567890</asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="32px" Width="288px">
					<asp:TextBox id="TextBox3" runat="server" Height="24px" Width="280px">`~!@#$%^%&amp;*()_+-=[]\{}|;':&quot;,./&lt;&gt;?</asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="32px" Width="288px">
					<asp:TextBox id="TextBox4" runat="server" Height="24px" Width="280px"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest5" runat="server" Height="32px" Width="288px">
					<asp:TextBox id="TextBox5" runat="server" Height="24px" Width="280px"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest6" runat="server" Height="96px" Width="152px">
					<asp:TextBox id="TextBox6" runat="server" Height="72px" Width="136px" TextMode="MultiLine">asdfasdfasdfadfadsfasdfasdfasdfasdfasdfasdf</asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest7" runat="server" Height="96px" Width="152px">
					<asp:TextBox id="TextBox7" runat="server" Height="72px" Width="136px" TextMode="MultiLine" Wrap="False">asdfasdfasdfadfadsfasdfasdfasdfasdfasdfasdf</asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest8" runat="server" Height="42px" Width="152px">
					<asp:TextBox id="TextBox8" runat="server" Height="26px" Width="136px" TextMode="Password" Wrap="False">abcdefg&amp;*</asp:TextBox>
				</cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
