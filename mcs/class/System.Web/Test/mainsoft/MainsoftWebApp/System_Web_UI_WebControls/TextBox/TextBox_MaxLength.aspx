<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TextBox_MaxLength.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TextBox_MaxLength" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TextBox_MaxLength</title>
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
					<asp:TextBox id="TextBox2" runat="server" MaxLength="1"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox3" runat="server" MaxLength="100"></asp:TextBox>
				</cc1:GHTSubTest>&nbsp;</P>
			<br>
			<P>
				<cc1:GHTSubTest id="GHTSubTest6" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox6" runat="server" TextMode="Password"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest5" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox5" runat="server" MaxLength="1" TextMode="Password"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox4" runat="server" MaxLength="100" TextMode="Password"></asp:TextBox>
				</cc1:GHTSubTest>&nbsp;</P>
			<BR>
			<P>
				<cc1:GHTSubTest id="GHTSubTest9" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox9" runat="server" TextMode="MultiLine"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest8" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox8" runat="server" MaxLength="1" TextMode="MultiLine"></asp:TextBox>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest7" runat="server" Height="32px" Width="168px">
					<asp:TextBox id="TextBox7" runat="server" MaxLength="100" TextMode="MultiLine"></asp:TextBox>
				</cc1:GHTSubTest>&nbsp;</P>
		</form>
		<BR>
		<BR>
	</body>
</HTML>
