<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="ListBox_SelectionMode.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.ListBox_SelectionMode" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>ListBox_SelectionMode</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="88px" Width="96px" DESIGNTIMEDRAGDROP="8">
				<asp:ListBox id="ListBox1" runat="server"></asp:ListBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="88px" Width="96px">
				<asp:ListBox id="ListBox2" runat="server" SelectionMode="Single"></asp:ListBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="88px" Width="96px">
				<asp:ListBox id="ListBox3" runat="server" SelectionMode="Multiple">
					<asp:ListItem></asp:ListItem>
					<asp:ListItem Selected="True"></asp:ListItem>
					<asp:ListItem Selected="True"></asp:ListItem>
					<asp:ListItem Selected="True"></asp:ListItem>
				</asp:ListBox>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
