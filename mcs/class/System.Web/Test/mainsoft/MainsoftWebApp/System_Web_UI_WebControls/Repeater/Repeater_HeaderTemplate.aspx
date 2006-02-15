<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Repeater_HeaderTemplate.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Repeater_HeaderTemplate" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Repeater_HeaderTemplate</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="553px" Height="24px">
				<asp:Repeater id="Repeater1" runat="server">
					<HeaderTemplate>
					</HeaderTemplate>
					<ItemTemplate>
						<div><%# Container.DataItem %></div>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="24px" Width="553px">
				<asp:Repeater id="Repeater2" runat="server">
					<HeaderTemplate>
						plain text header template
					</HeaderTemplate>
					<ItemTemplate>
						<div><%# Container.DataItem %></div>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="24px" Width="553px">
				<asp:Repeater id="Repeater3" runat="server">
					<HeaderTemplate>
						<div><span><b><i>html template</i></b></span></div>
					</HeaderTemplate>
					<ItemTemplate>
						<div><%# Container.DataItem %></div>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="24px" Width="553px">
				<asp:Repeater id="Repeater4" runat="server">
					<HeaderTemplate>
						<asp:Button Width="100px" Runat="server" Text="Server control template"></asp:Button>
					</HeaderTemplate>
					<ItemTemplate>
						<div><%# Container.DataItem %></div>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" runat="server" Height="24px" Width="553px">
				<asp:Repeater id="Repeater5" runat="server">
					<HeaderTemplate>
						<table>
							<tr>
								<td>separated html tag template</td>
							</tr>
					</HeaderTemplate>
					<ItemTemplate>
						<tr>
							<td><%# Container.DataItem %></td>
						</tr>
					</ItemTemplate>
					<FooterTemplate>
						</table>
					</FooterTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<br>
		</form>
	</body>
</HTML>
