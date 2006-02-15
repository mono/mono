<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DropDownList_BorderColor.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DropDownList_BorderColor" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DropDownList_BorderColor</title>
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
				runat="server" Width="128px" Height="32px">
				<asp:DropDownList id="DropDownList1" runat="server" Width="120px" BorderColor="#ff00ff"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" style="Z-INDEX: 105; LEFT: 16px; POSITION: absolute; TOP: 232px"
				runat="server" Height="32px" Width="128px">
				<asp:DropDownList id="DropDownList5" runat="server" Width="120px" BorderColor="#be7b1a"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 184px"
				runat="server" Height="32px" Width="128px">
				<asp:DropDownList id="DropDownList4" runat="server" Width="120px" BorderColor="Navy"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 128px"
				runat="server" Height="32px" Width="128px">
				<asp:DropDownList id="DropDownList3" runat="server" Width="120px" BorderColor="LimeGreen"></asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 72px"
				runat="server" Height="32px" Width="128px">
				<asp:DropDownList id="DropDownList2" runat="server" Width="120px" BorderColor="#d4d0c8"></asp:DropDownList>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
