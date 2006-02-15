<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Repeater_DataSource.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Repeater_DataSource" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Repeater_DataSource</title>
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
			<cc1:ghtsubtest id="GHTSubTest1" runat="server" Height="88px" Width="553px">
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
			</cc1:ghtsubtest><cc1:ghtsubtest id="Ghtsubtest2" runat="server" Height="88px" Width="553px">
				<asp:Repeater id="Repeater2" runat="server">
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
			</cc1:ghtsubtest><cc1:ghtsubtest id="Ghtsubtest3" runat="server" Height="88px" Width="553px">
				<asp:Repeater id="Repeater3" runat="server">
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
			</cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest4" runat="server" Width="553px" Height="88px">
				<asp:Repeater id="Repeater4" runat="server">
					<HeaderTemplate>
						<div>Header</div>
					</HeaderTemplate>
					<ItemTemplate>
						<%# Container.DataItem %>
					</ItemTemplate>
					<FooterTemplate>
						<div>Footer</div>
					</FooterTemplate>
				</asp:Repeater>
			</cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest5" runat="server" Width="553px" Height="88px">
				<asp:Repeater id="Repeater5" runat="server">
					<HeaderTemplate>
						<div>Header</div>
					</HeaderTemplate>
					<ItemTemplate>
						<%# Container.DataItem %>
					</ItemTemplate>
					<FooterTemplate>
						<div>Footer</div>
					</FooterTemplate>
				</asp:Repeater>
			</cc1:ghtsubtest>
			<cc1:GHTSubTest id="GHTSubTest6" runat="server" Width="384px" Height="91px">
				<asp:Repeater id="Repeater6" runat="server">
					<HeaderTemplate>
						<div>Header</div>
					</HeaderTemplate>
					<ItemTemplate>
						<%# Container.DataItem %>
					</ItemTemplate>
					<FooterTemplate>
						<div>Footer</div>
					</FooterTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest></form>
	</body>
</HTML>
