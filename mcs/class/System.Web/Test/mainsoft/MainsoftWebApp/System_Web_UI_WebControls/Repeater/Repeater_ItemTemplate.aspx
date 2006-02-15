<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Repeater_ItemTemplate.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Repeater_ItemTemplate" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Repeater_ItemTemplate</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="553px" Height="72px">
				<asp:Repeater id="Repeater1" runat="server">
					<ItemTemplate>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>&nbsp;
			<br>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="72px" Width="553px">
				<asp:Repeater id="Repeater2" runat="server">
					<ItemTemplate>
						plain text
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="72px" Width="553px">
				<asp:Repeater id="Repeater3" runat="server">
					<ItemTemplate>
						<b>html</b>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="72px" Width="553px">
				<asp:Repeater id="Repeater4" runat="server">
					<ItemTemplate>
						<asp:Button Width="100px" Runat="server" Text="Server control template" ID="Button1"></asp:Button>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" runat="server" Height="72px" Width="553px">
				<asp:Repeater id="Repeater5" runat="server">
					<ItemTemplate>
						<%# Container.DataItem %>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" runat="server" Height="72px" Width="553px">
				<asp:Repeater id="Repeater6" runat="server">
					<ItemTemplate>
						<asp:CheckBox runat="server" Text="<%# Container.DataItem %>">
						</asp:CheckBox>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest7" runat="server" Height="72px" Width="553px">
				<asp:Repeater id="Repeater7" runat="server">
					<ItemTemplate>
						<%# Container.DataItem %>
						<%# Container.DataItem %>
						<%# Container.DataItem %>
						<%# Container.DataItem %>
						<%# Container.DataItem %>
						<%# Container.DataItem %>
						<%# Container.DataItem %>
					</ItemTemplate>
				</asp:Repeater>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
