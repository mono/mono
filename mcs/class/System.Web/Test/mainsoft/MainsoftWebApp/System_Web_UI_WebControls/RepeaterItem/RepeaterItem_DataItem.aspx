<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="RepeaterItem_DataItem.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.RepeaterItem_DataItem" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>RepeaterItem_DataItem</title>
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
			<cc1:ghtsubtest id="GHTSubTest1" runat="server" Height="88px" Width="553px">
				<asp:Repeater id="Repeater1" runat="server">
					<HeaderTemplate>
						<div>
					</HeaderTemplate>
					<ItemTemplate>
						<div><%# Container.DataItem %></div>
					</ItemTemplate>
					<FooterTemplate>
						</div>
					</FooterTemplate>
				</asp:Repeater>
			</cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest2" runat="server" Width="553px" Height="88px">
				<asp:Repeater id="Repeater2" runat="server">
					<HeaderTemplate>
						<div>
					</HeaderTemplate>
					<ItemTemplate>
						<div><%# Container.DataItem %></div>
					</ItemTemplate>
					<FooterTemplate>
						</div>
					</FooterTemplate>
				</asp:Repeater>
			</cc1:ghtsubtest>&nbsp;
		</form>
	</body>
</HTML>
