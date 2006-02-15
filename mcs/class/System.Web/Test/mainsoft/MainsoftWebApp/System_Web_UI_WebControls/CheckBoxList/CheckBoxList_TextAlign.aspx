<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="CheckBoxList_TextAlign.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.CheckBoxList_TextAlign" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>CheckBoxList_TextAlign</title>
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
			<cc1:ghtsubtest id="GHTSubTest1" runat="server" Height="96px" Width="80px">
				<asp:CheckBoxList id="CheckBoxList1" runat="server" RepeatColumns="0">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
					<asp:ListItem Value="Item3">Item3</asp:ListItem>
					<asp:ListItem Value="Item4">Item4</asp:ListItem>
					<asp:ListItem Value="Item5">Item5</asp:ListItem>
					<asp:ListItem Value="Item6">Item6</asp:ListItem>
					<asp:ListItem Value="Item7">Item7</asp:ListItem>
				</asp:CheckBoxList>
			</cc1:ghtsubtest>&nbsp;
			<cc1:ghtsubtest id="Ghtsubtest2" runat="server" Height="96px" Width="80px">
				<asp:CheckBoxList id="CheckBoxList2" runat="server" RepeatColumns="0" TextAlign="Right">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
					<asp:ListItem Value="Item3">Item3</asp:ListItem>
					<asp:ListItem Value="Item4">Item4</asp:ListItem>
					<asp:ListItem Value="Item5">Item5</asp:ListItem>
					<asp:ListItem Value="Item6">Item6</asp:ListItem>
					<asp:ListItem Value="Item7">Item7</asp:ListItem>
				</asp:CheckBoxList>
			</cc1:ghtsubtest>&nbsp;
			<cc1:ghtsubtest id="Ghtsubtest3" runat="server" Height="96px" Width="80px">
				<asp:CheckBoxList id="CheckBoxList3" runat="server" RepeatColumns="0" TextAlign="Left">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
					<asp:ListItem Value="Item3">Item3</asp:ListItem>
					<asp:ListItem Value="Item4">Item4</asp:ListItem>
					<asp:ListItem Value="Item5">Item5</asp:ListItem>
					<asp:ListItem Value="Item6">Item6</asp:ListItem>
					<asp:ListItem Value="Item7">Item7</asp:ListItem>
				</asp:CheckBoxList>
			</cc1:ghtsubtest>
		</form>
		<br>
		<br>
	</body>
</HTML>
