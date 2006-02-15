<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HyperLink_Target.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.HyperLink_Target" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HyperLink_Target</title>
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
				runat="server" Width="88px" Height="40px">
				<asp:HyperLink id="HyperLink1" runat="server" Target="_blank">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest9" style="Z-INDEX: 109; LEFT: 16px; POSITION: absolute; TOP: 400px"
				runat="server" Height="40px" Width="88px">
				<asp:HyperLink id="HyperLink9" runat="server" Target="         ">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest8" style="Z-INDEX: 108; LEFT: 16px; POSITION: absolute; TOP: 352px"
				runat="server" Height="40px" Width="88px">
				<asp:HyperLink id="HyperLink8" runat="server" Target="">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest7" style="Z-INDEX: 106; LEFT: 16px; POSITION: absolute; TOP: 304px"
				runat="server" Height="40px" Width="88px">
				<asp:HyperLink id="HyperLink7" runat="server" Target="Frame1">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" style="Z-INDEX: 105; LEFT: 16px; POSITION: absolute; TOP: 256px"
				runat="server" Height="40px" Width="88px">
				<asp:HyperLink id="HyperLink6" runat="server" Target="_New">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 208px"
				runat="server" Height="40px" Width="88px">
				<asp:HyperLink id="HyperLink5" runat="server" Target="_top">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 160px"
				runat="server" Height="40px" Width="88px">
				<asp:HyperLink id="HyperLink4" runat="server" Target="_self">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 112px"
				runat="server" Height="40px" Width="88px">
				<asp:HyperLink id="HyperLink3" runat="server" Target="_search">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 64px"
				runat="server" Height="40px" Width="88px">
				<asp:HyperLink id="HyperLink2" runat="server" Target="_parent">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
