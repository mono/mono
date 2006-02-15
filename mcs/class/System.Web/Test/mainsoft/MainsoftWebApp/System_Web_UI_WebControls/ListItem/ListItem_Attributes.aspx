<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="ListItem_Attributes.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.ListItem_Attributes" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>ListItem_Attributes</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="120px" Width="553px">
					<asp:ListBox id="ListBox1" runat="server" Height="64px" Width="56px">
						<asp:ListItem Value="0">item 0</asp:ListItem>
						<asp:ListItem Value="1" AttributeKey="AttributeValue">item 1</asp:ListItem>
						<asp:ListItem Value="2" AttributeKey1="AttributeValue1" AttributeKey2="AttributeValue2" AttributeKey3="AttributeValue3"
							AttributeKey4="AttributeValue4" AttributeKey5="AttributeValue5" AttributeKey6="AttributeValue6"
							AttributeKey7="AttributeValue7" AttributeKey8="AttributeValue8" AttributeKey9="AttributeValue9"
							AttributeKey10="AttributeValue10" AttributeKey11="AttributeValue11" AttributeKey12="AttributeValue12"
							AttributeKey13="AttributeValue13">item 2</asp:ListItem>
					</asp:ListBox></P>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
