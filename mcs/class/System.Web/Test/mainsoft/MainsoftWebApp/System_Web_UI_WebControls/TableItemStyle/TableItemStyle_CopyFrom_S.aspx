<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableItemStyle_CopyFrom_S.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableItemStyle_CopyFrom_S" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableItemStyle_CopyFrom_S.aspx</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="Visual Basic .NET 7.1" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script language="JavaScript">
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
		<form id="Form2" method="post" runat="server">
			<P><asp:table id="Table2" runat="server" Width="256px" Height="120px">
					<asp:TableRow Width="100%" BorderWidth="8px" BorderColor="#804040" HorizontalAlign="Left" ForeColor="Olive">
						<asp:TableCell BorderStyle="Double" BorderWidth="4px" VerticalAlign="Bottom" BackColor="#C0FFC0"
							ForeColor="Blue" Height="50px" RowSpan="9" ColumnSpan="9" Font-Size="16pt" Font-Overline="True"
							Font-Underline="True" Font-Names="FreesiaUPC" Font-Italic="True" Font-Bold="True" Font-Strikeout="True"
							HorizontalAlign="Left" Wrap="False" BorderColor="#FF3333" Text="aaaaaa"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow VerticalAlign="Middle" BorderWidth="7px" BorderColor="#00C000" BorderStyle="Solid"
						HorizontalAlign="Center" ForeColor="#FF8000" BackColor="#C0C0FF" Font-Size="15pt" Font-Overline="True"
						Font-Underline="True" Font-Names="Comic Sans MS" Font-Italic="True" Font-Bold="True" Font-Strikeout="True">
						<asp:TableCell BackColor="#00C0C0" ForeColor="Purple" BorderColor="Silver" Text="aaaaaa"></asp:TableCell>
						<asp:TableCell BorderStyle="Dashed" BorderWidth="4px" BackColor="#FFFF80" ForeColor="#FFE0C0" Text="aaaaaa"></asp:TableCell>
						<asp:TableCell BorderStyle="Ridge" BorderWidth="7px" BackColor="#C0C0FF" ForeColor="Yellow" Text="aaaaaa"></asp:TableCell>
					</asp:TableRow>
				</asp:table></P>
			<P><br>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="536px" Height="200px">
					<asp:table id="Table1" runat="server" Height="152px" Width="304px">
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
				</cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
