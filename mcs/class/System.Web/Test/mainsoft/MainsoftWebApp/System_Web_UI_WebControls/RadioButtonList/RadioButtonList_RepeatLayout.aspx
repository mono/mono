<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="RadioButtonList_RepeatLayout.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.RadioButtonList_RepeatLayout" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>RadioButtonList_RepeatLayout</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="88px" Height="88px">
					<asp:RadioButtonList id="RadioButtonList1" runat="server">
						<asp:ListItem Value="item1">item1</asp:ListItem>
						<asp:ListItem Value="item2">item2</asp:ListItem>
						<asp:ListItem Value="item3">item3</asp:ListItem>
					</asp:RadioButtonList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="96px" Width="88px">
					<asp:RadioButtonList id="RadioButtonList2" runat="server" Height="72px" Width="80px" RepeatLayout="Table">
						<asp:ListItem Value="item1">item1</asp:ListItem>
						<asp:ListItem Value="item2">item2</asp:ListItem>
						<asp:ListItem Value="item3">item3</asp:ListItem>
					</asp:RadioButtonList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="96px" Width="88px">
					<asp:RadioButtonList id="RadioButtonList3" runat="server" Height="72px" Width="80px" RepeatLayout="Flow">
						<asp:ListItem Value="item1">item1</asp:ListItem>
						<asp:ListItem Value="item2">item2</asp:ListItem>
						<asp:ListItem Value="item3">item3</asp:ListItem>
					</asp:RadioButtonList>
				</cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
