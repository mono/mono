<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="RepeaterItem_ItemType_Manual.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.RepeaterItem_ItemType_Manual" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>RepeaterItem_ItemType_Manual</title>
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
				<cc1:ghtsubtest id="GHTSubTest1" runat="server" Width="553px" Height="88px">
					<asp:Repeater id="Repeater1" runat="server">
						<HeaderTemplate>
							<div>
						</HeaderTemplate>
						<ItemTemplate>
							<%# Container.DataItem %>
						</ItemTemplate>
						<FooterTemplate>
							</div>
						</FooterTemplate>
					</asp:Repeater>
				</cc1:ghtsubtest></P>
		</form>
	</body>
</HTML>
