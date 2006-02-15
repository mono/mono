<%@ Page Language="c#" AutoEventWireup="false" Codebehind="TableStyle_HorizontalAlign.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.TableStyle_HorizontalAlign" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>TableStyle_HorizontalAlign</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="120px" Width="296px">
					<asp:Table id="Table1" runat="server" Width="160px" Height="96px"></asp:Table>
				</cc1:GHTSubTest>&nbsp;</P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="120px" Width="296px">
					<asp:Table id="Table2" runat="server" Width="160px" Height="96px"></asp:Table>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="120px" Width="296px">
					<asp:Table id="Table3" runat="server" Width="160px" Height="96px"></asp:Table>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="120px" Width="296px">
					<asp:Table id="Table4" runat="server" Width="160px" Height="96px"></asp:Table>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest5" runat="server" Height="120px" Width="296px">
					<asp:Table id="Table5" runat="server" Width="160px" Height="96px"></asp:Table>
				</cc1:GHTSubTest></P>
			<P>&nbsp;</P>
		</form>
	</body>
</HTML>
