<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HyperLink_ImageUrl.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.HyperLink_ImageUrl" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HyperLink_ImageUrl</title>
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
				runat="server" Width="128px" Height="64px">
				<asp:HyperLink id="HyperLink1" runat="server" ImageUrl="http://localhost:/GHTTests/System_Web_dll/System_Web_UI_WebControls/HyperLink/Car.jpg">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 216px"
				runat="server" Height="64px" Width="128px">
				<asp:HyperLink id="HyperLink4" runat="server" ImageUrl="">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 144px"
				runat="server" Height="64px" Width="128px">
				<asp:HyperLink id="HyperLink3" runat="server" ImageUrl="/GHTTests/System_Web_dll/System_Web_UI_WebControls/HyperLink/Car.jpg">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 80px"
				runat="server" Height="64px" Width="128px">
				<asp:HyperLink id="HyperLink2" runat="server" ImageUrl="Car.jpg">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
