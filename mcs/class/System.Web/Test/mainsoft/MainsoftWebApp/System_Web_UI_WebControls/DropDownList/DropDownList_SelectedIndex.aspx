<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DropDownList_SelectedIndex.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DropDownList_SelectedIndex" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DropDownList_SelectedIndex</title>
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
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="40px" Width="96px">
				<asp:DropDownList id="DropDownList2" runat="server" Width="88px">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
					<asp:ListItem Value="Item3" Selected="True">Item3</asp:ListItem>
					<asp:ListItem Value="Item4" Selected="False">Item4</asp:ListItem>
					<asp:ListItem Value="Item5">Item5</asp:ListItem>
					<asp:ListItem Value="Item6">Item6</asp:ListItem>
				</asp:DropDownList>
			</cc1:GHTSubTest>&nbsp;
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="96px" Height="32px">
				<asp:DropDownList id="DropDownList1" runat="server" Width="88px">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
					<asp:ListItem Value="Item3">Item3</asp:ListItem>
					<asp:ListItem Value="Item4" Selected="True">Item4</asp:ListItem>
					<asp:ListItem Value="Item5">Item5</asp:ListItem>
					<asp:ListItem Value="Item6">Item6</asp:ListItem>
				</asp:DropDownList>
			</cc1:GHTSubTest>
		</form>
		<br>
		<br>
	</body>
</HTML>
