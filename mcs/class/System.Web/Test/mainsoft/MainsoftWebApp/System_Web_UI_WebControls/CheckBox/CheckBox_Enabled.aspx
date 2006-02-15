<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="CheckBox_Enabled.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.CheckBox_Enabled" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>CheckBox_Enabled</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="48px" Width="144px" Description="Enabled TextBox">
					<asp:CheckBox id="CheckBox1" runat="server" Text="Enabled text"></asp:CheckBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="144px" Height="40px" Description="Disabled TextBox">
					<asp:CheckBox id="CheckBox2" runat="server" Text="Disabled text" Enabled="False"></asp:CheckBox>
				</cc1:GHTSubTest>&nbsp;</P>
		</form>
	</body>
</HTML>
