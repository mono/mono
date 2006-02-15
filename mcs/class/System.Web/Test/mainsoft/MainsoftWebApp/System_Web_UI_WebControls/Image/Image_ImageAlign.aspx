<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Image_ImageAlign.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Image_ImageAlign" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Image_ImageAlign</title>
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
				runat="server" Width="40px" Height="40px">
				<asp:Image id="Image1" runat="server" ImageAlign="NotSet"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest10" style="Z-INDEX: 110; LEFT: 88px; POSITION: absolute; TOP: 208px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image10" runat="server" ImageAlign="TextTop"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest9" style="Z-INDEX: 109; LEFT: 88px; POSITION: absolute; TOP: 160px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image9" runat="server" ImageAlign="AbsMiddle"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest8" style="Z-INDEX: 107; LEFT: 88px; POSITION: absolute; TOP: 112px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image8" runat="server" ImageAlign="AbsBottom"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest7" style="Z-INDEX: 106; LEFT: 88px; POSITION: absolute; TOP: 64px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image7" runat="server" ImageAlign="Bottom"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" style="Z-INDEX: 105; LEFT: 88px; POSITION: absolute; TOP: 16px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image6" runat="server" ImageAlign="Middle"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 208px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image5" runat="server" ImageAlign="Top"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 103; LEFT: 16px; POSITION: absolute; TOP: 160px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image4" runat="server" ImageAlign="Baseline"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 112px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image3" runat="server" ImageAlign="Right"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 64px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image2" runat="server" ImageAlign="Left"></asp:Image>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
