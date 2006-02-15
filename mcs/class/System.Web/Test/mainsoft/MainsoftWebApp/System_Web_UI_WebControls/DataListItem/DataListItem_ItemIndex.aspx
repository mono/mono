<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataListItem_ItemIndex.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataListItem_ItemIndex" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataListItem_ItemIndex</title>
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
			<asp:datalist id=DataList1 runat="server" DataSource="<%# m_data %>" EditItemIndex="2" SelectedIndex="3">
				<HeaderTemplate>
					Header
				</HeaderTemplate>
				<FooterTemplate>
					Footer
				</FooterTemplate>
				<ItemTemplate>
					<%#Container.DataItem%>
				</ItemTemplate>
				<SeparatorTemplate>
					Seperator
				</SeparatorTemplate>
			</asp:datalist>&nbsp;
			<cc1:ghtsubtest id="GHTSubTest1" runat="server" Width="144px" Height="96px" Description="AlternatingItem"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest2" runat="server" Width="144px" Height="96px" Description="EditItem"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest3" runat="server" Width="144px" Height="96px" Description="FooterItem"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest4" runat="server" Width="144px" Height="96px" Description="Header"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest5" runat="server" Width="144px" Height="96px" Description="Item"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest6" runat="server" Width="144px" Height="96px" Description="Pager"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest7" runat="server" Width="144px" Height="96px" Description="SelectedItem"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest8" runat="server" Width="144px" Height="96px" Description="Separator"></cc1:ghtsubtest></form>
	</body>
</HTML>
