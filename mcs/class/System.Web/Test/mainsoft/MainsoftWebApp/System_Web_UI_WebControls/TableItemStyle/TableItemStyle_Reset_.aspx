<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableItemStyle_Reset_.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableItemStyle_Reset_" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableItemStyle_Reset_</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 100; LEFT: 16px; POSITION: absolute; TOP: 15px"
				runat="server" Width="553px" Height="208px">
				<P>&nbsp;</P>
				<asp:Table id="Table1" runat="server" Height="128px" Width="272px">
					<asp:TableRow BorderWidth="16px" VerticalAlign=Middle  BackColor="#FFEFC0" ForeColor="#A0FFCF">
						<asp:TableCell BorderStyle="Ridge" Font-Overline="True" BorderWidth="2px" VerticalAlign="Middle"
							BackColor="#FFE0C0" ForeColor="#C0FFC0" Font-Italic="True" Font-Bold="True" HorizontalAlign="Left"
							BorderColor="#C00000" Text="blablabla"></asp:TableCell>
						<asp:TableCell BorderWidth="6px" VerticalAlign="Bottom" BackColor="#FFE0C0" ForeColor="#A0FFC0"
							Font-Bold="True" HorizontalAlign="Left" BorderColor="#C00600" Text="blablabla"></asp:TableCell>
						<asp:TableCell BorderWidth="1px" VerticalAlign="Top" BackColor="#FFE0C0" ForeColor="#C0FFC0" Font-Italic="True"
							BorderStyle="Dotted" HorizontalAlign="Left" BorderColor="#C70000" Text="blablabla"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow BorderWidth="6px" VerticalAlign="Bottom" BackColor="#EEE0C0" ForeColor="#AFFF55"
							Font-Bold="True" HorizontalAlign="Left" BorderColor="#C00600">
							<asp:TableCell  Text="blablabla"></asp:TableCell>
						<asp:TableCell Text="blablabla"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
		</form>
		<P>
			<br>
			&nbsp;</P>
		<P>&nbsp;</P>
		<P>&nbsp;</P>
		<P>&nbsp;</P>
		<P>&nbsp;</P>
		<P>&nbsp;</P>
		<P>&nbsp;</P>
	</body>
</HTML>
