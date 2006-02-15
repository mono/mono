<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableCell_RowSpan.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableCell_RowSpan" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableCell_RowSpan</title>
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
			<p>
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="176px" Height="72px">
					<asp:Table id="Table1" runat="server" Height="120px" Width="248px">
						<asp:TableRow>
							<asp:TableCell RowSpan="50">123</asp:TableCell>
							<asp:TableCell RowSpan="12">123</asp:TableCell>
						</asp:TableRow>
						<asp:TableRow>
							<asp:TableCell RowSpan="30">123</asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				</cc1:GHTSubTest>
			</p>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="72px" Width="176px">
					<asp:Table id="Table2" runat="server" Height="120px" Width="248px">
						<asp:TableRow>
							<asp:TableCell RowSpan="0">123</asp:TableCell>
							<asp:TableCell RowSpan="0">123</asp:TableCell>
						</asp:TableRow>
						<asp:TableRow>
							<asp:TableCell>123</asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="72px" Width="176px">
					<asp:Table id="Table3" runat="server" Height="120px" Width="248px">
						<asp:TableRow>
							<asp:TableCell>123</asp:TableCell>
						</asp:TableRow>
						<asp:TableRow>
							<asp:TableCell>123</asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				</cc1:GHTSubTest></P>
		</form>
		<br>
		<br>
	</body>
</HTML>
