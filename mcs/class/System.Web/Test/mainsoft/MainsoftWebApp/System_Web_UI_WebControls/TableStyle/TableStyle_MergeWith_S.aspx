<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableStyle_MergeWith_S.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableStyle_MergeWith_S" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableStyle_MergeWith_S</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 100; LEFT: 112px; POSITION: absolute; TOP: 15px"
				runat="server" Width="457px" Height="160px">
				<asp:Table id="Table1" runat="server" GridLines="Horizontal" BorderWidth="5px" BorderColor="Green"
					BackColor="#C0C0FF">
					<asp:TableRow>
						<asp:TableCell Text="qqqqqq"></asp:TableCell>
						<asp:TableCell></asp:TableCell>
						<asp:TableCell Text="qqqq"></asp:TableCell>
						<asp:TableCell></asp:TableCell>
						<asp:TableCell Text="qqq"></asp:TableCell>
						<asp:TableCell></asp:TableCell>
						<asp:TableCell></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell></asp:TableCell>
						<asp:TableCell Text="qqqq"></asp:TableCell>
						<asp:TableCell></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow></asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
		</form>
		<P>&nbsp;</P>
		<P>&nbsp;</P>
		<P>&nbsp;</P>
		<P>
			<br>
			<br>
			&nbsp;</P>
		<asp:Table id="Table2" runat="server" BackColor="#FFFF80" BorderColor="#FF8080" BorderWidth="10px"
			BorderStyle="Dotted" CellPadding="5" CellSpacing="5" Font-Bold="True" Font-Overline="True"
			ForeColor="Purple" GridLines="Vertical" Font-Names="Aharoni" Font-Size="18pt">
			<asp:TableRow>
				<asp:TableCell Text="qqqqqq"></asp:TableCell>
				<asp:TableCell></asp:TableCell>
				<asp:TableCell Text="qqqq"></asp:TableCell>
				<asp:TableCell></asp:TableCell>
				<asp:TableCell Text="qqq"></asp:TableCell>
				<asp:TableCell></asp:TableCell>
				<asp:TableCell></asp:TableCell>
			</asp:TableRow>
			<asp:TableRow>
				<asp:TableCell></asp:TableCell>
				<asp:TableCell Text="qqqq"></asp:TableCell>
				<asp:TableCell></asp:TableCell>
			</asp:TableRow>
			<asp:TableRow></asp:TableRow>
		</asp:Table>
	</body>
</HTML>
