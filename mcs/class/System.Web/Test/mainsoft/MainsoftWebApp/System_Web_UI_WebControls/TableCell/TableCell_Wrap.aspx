<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableCell_Wrap.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableCell_Wrap" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableCell_Wrap</title>
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
				runat="server" Width="553px" Height="296px">
				<asp:Table id="Table1" runat="server" Height="200px" Width="48px">
					<asp:TableRow>
						<asp:TableCell Wrap="False">*A 123456790123456790123456790123456790123456790123456790123456790123456790*</asp:TableCell>
						<asp:TableCell Wrap="True">*B 123456790123456790123456790123456790123456790123456790123456790123456790*</asp:TableCell>
						<asp:TableCell>*C 123456790123456790123456790123456790123456790123456790123456790123456790*</asp:TableCell>
						<asp:TableCell Wrap="True">*D 123456790				123456790				123456790			123456790*</asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell Wrap="True">*D 	123456790	123456790	123456790	123456790*</asp:TableCell>
						<asp:TableCell Wrap="True">*E  123456790   123456790   123456790   123456790*</asp:TableCell>
						<asp:TableCell Wrap="False">*F 123456790123456790123456790123456790123456790123456790123456790123456790*</asp:TableCell>
						<asp:TableCell Wrap="False">*G 123456790123456790123456790123456790123456790123456790123456790123456790*</asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
		</form>
		<br>
		<br>
	</body>
</HTML>
