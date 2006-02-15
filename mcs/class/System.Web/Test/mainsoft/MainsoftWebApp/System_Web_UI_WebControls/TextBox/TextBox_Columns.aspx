<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TextBox_Columns.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TextBox_Columns" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TextBox_Columns</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="168px" Height="32px">
					<asp:TextBox id="TextBox1" runat="server"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox2" runat="server" Columns="1"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox3" runat="server" Columns="10"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox4" runat="server" Columns="100"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>&nbsp;</P>
		</form>
		<br>
		<br>
	</body>
</HTML>
