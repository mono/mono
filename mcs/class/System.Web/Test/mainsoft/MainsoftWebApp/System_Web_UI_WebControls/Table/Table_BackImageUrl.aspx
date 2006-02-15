<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Table_BackImageUrl.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Table_BackImageUrl" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Table_BackImageUrl</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 100; LEFT: 16px; POSITION: absolute; TOP: 16px"
					runat="server" Width="553px" Height="296px">
					<asp:Table id="Table1" runat="server" Height="176px" Width="360px" BackImageUrl="myImage.jpg">
						<asp:TableRow>
							<asp:TableCell>1</asp:TableCell>
							<asp:TableCell>2</asp:TableCell>
						</asp:TableRow>
						<asp:TableRow>
							<asp:TableCell>3</asp:TableCell>
							<asp:TableCell>4</asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				</cc1:GHTSubTest>
			<P>&nbsp;</P>
			<P>&nbsp;</P>
			<P>&nbsp;</P>
			<P>&nbsp;</P>
			<P>&nbsp;</P>
			<P>&nbsp;</P>
			<P>&nbsp;</P>
			<P>&nbsp;</P>
			<P>&nbsp;</P>
			<P>&nbsp;</P>
			<cc1:GHTSubTest id="Ghtsubtest2" runat="server" Width="553px" Height="296px">
				<asp:Table id="Table2" runat="server" Height="176px" Width="360px" BackImageUrl="!@##$%$^%^&amp;*(myImage.&amp;^**&amp;jpg">
					<asp:TableRow>
						<asp:TableCell>1</asp:TableCell>
						<asp:TableCell>2</asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell>3</asp:TableCell>
						<asp:TableCell>4</asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="Ghtsubtest3" runat="server" Width="553px" Height="296px">
				<asp:Table id="Table3" runat="server" Height="176px" Width="360px" BackImageUrl="c:\temp\test.jshmeg">
					<asp:TableRow>
						<asp:TableCell>1</asp:TableCell>
						<asp:TableCell>2</asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell>3</asp:TableCell>
						<asp:TableCell>4</asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
