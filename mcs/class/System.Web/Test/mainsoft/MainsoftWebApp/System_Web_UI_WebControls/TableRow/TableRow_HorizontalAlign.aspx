<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableRow_HorizontalAlign.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableRow_HorizontalAlign" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableRow_HorizontalAlign</title>
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
				runat="server" Width="128px" Height="56px">
				<asp:Table id="Table1" runat="server" Height="40px" Width="88px">
					<asp:TableRow>
						<asp:TableCell Text="111"></asp:TableCell>
						<asp:TableCell Text="222"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>&nbsp;
		</form>
		<P>
			<br>
			<br>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="56px" Width="128px">
				<asp:Table id="Table2" runat="server" Height="40px" Width="88px">
					<asp:TableRow HorizontalAlign="Left">
						<asp:TableCell Text="111"></asp:TableCell>
						<asp:TableCell Text="222"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest></P>
		<P>
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="56px" Width="128px">
				<asp:Table id="Table3" runat="server" Height="40px" Width="88px">
					<asp:TableRow HorizontalAlign="Center">
						<asp:TableCell Text="111"></asp:TableCell>
						<asp:TableCell Text="222"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest></P>
		<P>
			<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="56px" Width="128px">
				<asp:Table id="Table4" runat="server" Height="40px" Width="88px">
					<asp:TableRow HorizontalAlign="Right">
						<asp:TableCell Text="111"></asp:TableCell>
						<asp:TableCell Text="222"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest></P>
		<P>
			<cc1:GHTSubTest id="GHTSubTest5" runat="server" Height="56px" Width="128px">
				<asp:Table id="Table5" runat="server" Height="40px" Width="88px">
					<asp:TableRow HorizontalAlign="Justify">
						<asp:TableCell Text="111"></asp:TableCell>
						<asp:TableCell Text="222"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest></P>
	</body>
</HTML>
