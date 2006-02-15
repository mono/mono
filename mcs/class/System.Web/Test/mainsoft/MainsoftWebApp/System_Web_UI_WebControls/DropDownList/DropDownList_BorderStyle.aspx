<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DropDownList_BorderStyle.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DropDownList_BorderStyle" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DropDownList_BorderStyle</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 100; LEFT: 16px; POSITION: absolute; TOP: 15px"
				runat="server" Width="120px" Height="32px">
				<asp:DropDownList id="DropDownList1" runat="server" Width="96px" BorderStyle="Dashed"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest10" style="Z-INDEX: 110; LEFT: 16px; POSITION: absolute; TOP: 376px"
				runat="server" Height="32px" Width="120px">
				<asp:DropDownList id="DropDownList10" runat="server" Width="96px" BorderStyle="Dotted"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest7" style="Z-INDEX: 106; LEFT: 16px; POSITION: absolute; TOP: 256px"
				runat="server" Height="32px" Width="120px">
				<asp:DropDownList id="DropDownList7" runat="server" Width="96px" BorderStyle="Double"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" style="Z-INDEX: 105; LEFT: 16px; POSITION: absolute; TOP: 216px"
				runat="server" Height="32px" Width="120px">
				<asp:DropDownList id="DropDownList6" runat="server" Width="96px" BorderStyle="Groove"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 176px"
				runat="server" Height="32px" Width="120px">
				<asp:DropDownList id="DropDownList5" runat="server" Width="96px" BorderStyle="Inset"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 136px"
				runat="server" Height="32px" Width="120px">
				<asp:DropDownList id="DropDownList4" runat="server" Width="96px" BorderStyle="None"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 96px"
				runat="server" Height="32px" Width="120px">
				<asp:DropDownList id="DropDownList3" runat="server" Width="96px" BorderStyle="NotSet"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 56px"
				runat="server" Height="32px" Width="120px">
				<asp:DropDownList id="DropDownList2" runat="server" Width="96px" BorderStyle="Outset"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest9" style="Z-INDEX: 109; LEFT: 16px; POSITION: absolute; TOP: 336px"
				runat="server" Height="32px" Width="120px">
				<asp:DropDownList id="DropDownList9" runat="server" Width="96px" BorderStyle="Ridge"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest8" style="Z-INDEX: 108; LEFT: 16px; POSITION: absolute; TOP: 296px"
				runat="server" Height="32px" Width="120px">
				<asp:DropDownList id="DropDownList8" runat="server" Width="96px" BorderStyle="Solid"></asp:DropDownList>
			</cc1:GHTSubTest>
		</form>
		<br>
		<br>
	</body>
</HTML>
