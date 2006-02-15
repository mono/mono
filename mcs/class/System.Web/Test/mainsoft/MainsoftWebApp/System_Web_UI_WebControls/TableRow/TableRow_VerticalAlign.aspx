<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableRow_VerticalAlign.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableRow_VerticalAlign" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableRow_VerticalAlign</title>
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
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="56px" Width="128px">
					<asp:Table id="Table4" runat="server" Width="88px" Height="40px">
						<asp:TableRow>
							<asp:TableCell Text="111"></asp:TableCell>
							<asp:TableCell Text="222"></asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="56px" Width="128px">
					<asp:Table id="Table1" runat="server" Width="88px" Height="40px">
						<asp:TableRow VerticalAlign="Top">
							<asp:TableCell Text="111"></asp:TableCell>
							<asp:TableCell Text="222"></asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="56px" Width="128px">
					<asp:Table id="Table2" runat="server" Width="88px" Height="40px">
						<asp:TableRow VerticalAlign="Middle">
							<asp:TableCell Text="111"></asp:TableCell>
							<asp:TableCell Text="222"></asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="56px" Width="128px">
					<asp:Table id="Table3" runat="server" Width="88px" Height="40px">
						<asp:TableRow VerticalAlign="Bottom">
							<asp:TableCell Text="111"></asp:TableCell>
							<asp:TableCell Text="222"></asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				</cc1:GHTSubTest></P>
			<P>&nbsp;</P>
		</form>
		<br>
		<br>
	</body>
</HTML>
