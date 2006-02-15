<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Literal_Text.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Literal_Text" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
    <HEAD>
        <title>Literal_Text</title>
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
				runat="server" Width="120px" Height="56px">
				<asp:Literal id="Literal1" runat="server" Text="   abcdefghijklm nopqrstuvwxyz       "></asp:Literal>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" style="Z-INDEX: 106; LEFT: 24px; POSITION: absolute; TOP: 312px"
				runat="server" Height="40px" Width="192px">
				<asp:Literal id="Literal6" runat="server" Text="                          "></asp:Literal>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" style="Z-INDEX: 105; LEFT: 24px; POSITION: absolute; TOP: 256px"
				runat="server" Height="40px" Width="192px">
				<asp:Literal id="Literal5" runat="server"></asp:Literal>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 200px"
				runat="server" Height="40px" Width="192px">
				<asp:Literal id="Literal4" runat="server" Text='~!@#$%^&amp;*()_+{}|:"<>?`[]\-=,./'></asp:Literal>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" style="Z-INDEX: 102; LEFT: 16px; POSITION: absolute; TOP: 144px"
				runat="server" Height="40px" Width="192px">
				<asp:Literal id="Literal3" runat="server" Text="134567890"></asp:Literal>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 88px"
				runat="server" Height="40px" Width="192px">
				<asp:Literal id="Literal2" runat="server" Text="   abcdefghijklm nopqrstuvwxyz       "></asp:Literal>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<br>
		<br>
	</body>
</HTML>
