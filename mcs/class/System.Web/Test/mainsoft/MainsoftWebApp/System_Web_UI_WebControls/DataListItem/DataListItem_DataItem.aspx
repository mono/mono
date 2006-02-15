<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataListItem_DataItem.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataListItem_DataItem" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataListItem_DataItem</title>
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
			<asp:datalist id=DataList1 runat="server" DataSource="<%# m_data %>" SelectedIndex="3" EditItemIndex="2">
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
			</asp:datalist>
			<cc1:ghtsubtest id="GHTSubTest1" runat="server" Description="AlternatingItem" Height="96px" Width="144px"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest2" runat="server" Description="EditItem" Height="96px" Width="144px"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest3" runat="server" Description="FooterItem" Height="96px" Width="144px"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest4" runat="server" Description="Header" Height="96px" Width="144px"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest5" runat="server" Description="Item" Height="96px" Width="144px"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest6" runat="server" Description="Pager" Height="96px" Width="144px"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest7" runat="server" Description="SelectedItem" Height="96px" Width="144px"></cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest8" runat="server" Description="Separator" Height="96px" Width="144px"></cc1:ghtsubtest>&nbsp;</form>
	</body>
</HTML>
