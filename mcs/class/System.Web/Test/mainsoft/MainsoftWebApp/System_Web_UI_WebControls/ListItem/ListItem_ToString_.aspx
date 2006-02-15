<%@ Page Language="c#" AutoEventWireup="false" Codebehind="ListItem_ToString_.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.ListItem_ToString_" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>ListItem_ToString_</title>
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
		<FORM id="Form1" method="post" runat="server">
			<P>
				<asp:RadioButtonList id="RadioButtonList1" runat="server" Width="128px">
					<asp:ListItem Value="   abcd efg hijkl nmopqrst   uvwxyz    "></asp:ListItem>
				</asp:RadioButtonList></P>
			<P>
				<asp:RadioButtonList id="RadioButtonList2" runat="server" Width="128px">
					<asp:ListItem Value="`1234567890-= ~!@#$%^&amp;*()_+ []\;',./ {}|:&quot;&lt;&gt;?"></asp:ListItem>
				</asp:RadioButtonList></P>
			<P>
				<asp:DropDownList id="DropDownList1" runat="server">
					<asp:ListItem Value="                               "></asp:ListItem>
				</asp:DropDownList></P>
			<P>
				<asp:DropDownList id="DropDownList2" runat="server">
					<asp:ListItem></asp:ListItem>
				</asp:DropDownList></P>
		</FORM>
	</body>
</HTML>
