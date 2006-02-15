<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="CheckBoxList_CellPadding.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.CheckBoxList_CellPadding" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>CheckBoxList_CellPadding</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="80px" Height="104px">
					<asp:CheckBoxList id="CheckBoxList1" runat="server">
						<asp:ListItem Value="Item1">Item1</asp:ListItem>
						<asp:ListItem Value="Item2">Item2</asp:ListItem>
						<asp:ListItem Value="Item3">Item3</asp:ListItem>
						<asp:ListItem Value="Item4">Item4</asp:ListItem>
					</asp:CheckBoxList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="104px" Width="80px">
					<asp:CheckBoxList id="CheckBoxList2" runat="server" CellPadding="0">
						<asp:ListItem Value="Item1">Item1</asp:ListItem>
						<asp:ListItem Value="Item2">Item2</asp:ListItem>
						<asp:ListItem Value="Item3">Item3</asp:ListItem>
						<asp:ListItem Value="Item4">Item4</asp:ListItem>
					</asp:CheckBoxList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="104px" Width="80px">
					<asp:CheckBoxList id="CheckBoxList3" runat="server" CellPadding="5">
						<asp:ListItem Value="Item1">Item1</asp:ListItem>
						<asp:ListItem Value="Item2">Item2</asp:ListItem>
						<asp:ListItem Value="Item3">Item3</asp:ListItem>
						<asp:ListItem Value="Item4">Item4</asp:ListItem>
					</asp:CheckBoxList>
				</cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
