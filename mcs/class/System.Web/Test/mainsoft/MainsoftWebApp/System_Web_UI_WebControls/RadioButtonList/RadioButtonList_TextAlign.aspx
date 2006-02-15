<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="RadioButtonList_TextAlign.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.RadioButtonList_TextAlign" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>RadioButtonList_TextAlign</title>
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
		<form id="Form1" method="post" runat="server">
			<P><cc1:ghtsubtest id="GHTSubTest1" runat="server" Height="80px" Width="96px">
					<asp:RadioButtonList id="RadioButtonList1" runat="server"></asp:RadioButtonList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="Ghtsubtest2" runat="server" Height="80px" Width="96px">
					<asp:RadioButtonList id="RadioButtonList2" runat="server" TextAlign="Left"></asp:RadioButtonList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="Ghtsubtest4" runat="server" Height="80px" Width="96px">
					<asp:RadioButtonList id="RadioButtonList6" runat="server" TextAlign="Right"></asp:RadioButtonList>
				</cc1:ghtsubtest></P>
		</form>
	</body>
</HTML>
