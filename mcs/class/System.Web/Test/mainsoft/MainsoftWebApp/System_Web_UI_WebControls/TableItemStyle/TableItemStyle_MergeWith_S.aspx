<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableItemStyle_MergeWith_S.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableItemStyle_MergeWith_S" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableItemStyle_MergeWith_S</title>
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
		<asp:Table id="Table1" runat="server" Height="144px" Width="272px">
			<asp:TableRow BorderStyle="Double" ForeColor="#804000" BackColor="#C0C0FF">
				<asp:TableCell Text="11111111"></asp:TableCell>
				<asp:TableCell BorderStyle="Dotted" BorderWidth="12px" VerticalAlign="Top" BackColor="#C0C000"
					ForeColor="Blue" RowSpan="2" ColumnSpan="3" Font-Size="12pt" Font-Overline="True" Font-Underline="True"
					Font-Names="Batang" Font-Italic="True" Font-Bold="True" Font-Strikeout="True" HorizontalAlign="Center"
					Wrap="False" Text="blablo"></asp:TableCell>
				<asp:TableCell Text="222222"></asp:TableCell>
				<asp:TableCell Text="33333333"></asp:TableCell>
			</asp:TableRow>
			<asp:TableRow BorderStyle="Ridge" ForeColor="#104040" BackColor="#C0AAFF">
				<asp:TableCell Text="11111111"></asp:TableCell>
				<asp:TableCell BorderStyle="Dotted" BackColor="#C0C000" ForeColor="Blue" HorizontalAlign="Center"
					Text="blablo"></asp:TableCell>
				<asp:TableCell Text="222222"></asp:TableCell>
				<asp:TableCell Text="33333333"></asp:TableCell>
			</asp:TableRow>
		</asp:Table>
		<P>&nbsp;</P>
		<P>
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="553px" Height="184px">
				<asp:Table id="Table2" Width="192px" Height="96px" runat="server">
					<asp:TableRow ForeColor="Gray" Font-Italic="True" Font-Bold="True">
						<asp:TableCell Text="1111"></asp:TableCell>
						<asp:TableCell BorderStyle="Ridge" BorderWidth="5px" Font-Size="9pt" Font-Bold="True" BorderColor="#804000"
							Text="22222"></asp:TableCell>
						<asp:TableCell Text="33333"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell Text="66666"></asp:TableCell>
						<asp:TableCell VerticalAlign="Top" ForeColor="Red" HorizontalAlign="Justify" Text="7777"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest><BR>
			<BR>
		</P>
	</body>
</HTML>
