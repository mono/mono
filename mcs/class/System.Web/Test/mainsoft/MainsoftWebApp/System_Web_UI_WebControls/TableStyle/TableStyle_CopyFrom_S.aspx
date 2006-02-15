<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableStyle_CopyFrom_S.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableStyle_CopyFrom_S" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableStyle_CopyFrom_S</title>
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
		</form>
		<br>
		<br>
		<asp:table id="Table2" runat="server" Height="120px" Width="256px" HorizontalAlign="Right"
			Font-Bold="True" Font-Italic="True" CellPadding="5" CellSpacing="8" BorderStyle="Groove"
			BorderColor="#C0FFFF" BackColor="#FFFF80" Font-Names="Symbol" Font-Overline="True" Font-Size="17pt"
			Font-Strikeout="True" Font-Underline="True" ForeColor="#C04000" GridLines="Both" BorderWidth="7px"
			BackImageUrl="http://yadayada.kada.com">
			<asp:TableRow Width="100%" BorderWidth="8px" BorderColor="#804040" HorizontalAlign="Left" ForeColor="Olive">
				<asp:TableCell BorderStyle="Double" BorderColor="#FF3333" Text="aaaaaa"></asp:TableCell>
			</asp:TableRow>
			<asp:TableRow VerticalAlign="Middle" BorderWidth="7px" BorderColor="#00C000" BorderStyle="Solid"
				HorizontalAlign="Center">
				<asp:TableCell BackColor="#00C0C0" ForeColor="Purple" BorderColor="Silver" Text="aaaaaa"></asp:TableCell>
				<asp:TableCell BorderStyle="Dashed" BorderWidth="4px" BackColor="#FFFF80" ForeColor="#FFE0C0" Text="aaaaaa"></asp:TableCell>
				<asp:TableCell BorderStyle="Ridge" BorderWidth="7px" BackColor="#C0C0FF" ForeColor="Yellow" Text="aaaaaa"></asp:TableCell>
			</asp:TableRow>
		</asp:table>
		<P></P>
		<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="200px" Width="536px">
			<asp:table id="Table1" runat="server" Width="304px" Height="152px">
				<asp:TableRow>
					<asp:TableCell Text="aaaaaa"></asp:TableCell>
					<asp:TableCell Text="aaaaaa"></asp:TableCell>
					<asp:TableCell Text="aaaaaa"></asp:TableCell>
				</asp:TableRow>
				<asp:TableRow>
					<asp:TableCell Text="aaaaaa"></asp:TableCell>
					<asp:TableCell Text="aaaaaa"></asp:TableCell>
				</asp:TableRow>
				<asp:TableRow>
					<asp:TableCell Text="aaaaaa"></asp:TableCell>
					<asp:TableCell Text="aaaaaa"></asp:TableCell>
					<asp:TableCell Text="aaaaaa"></asp:TableCell>
				</asp:TableRow>
			</asp:table>
		</cc1:GHTSubTest>
	</body>
</HTML>
