<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_RepeatDirection.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_RepeatDirection" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_RepeatDirection</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="96px" Width="144px" Description="Vertical - repeatColumn=0">
					<asp:DataList id=DataList1 runat="server" DataSource="<%# m_data %>">
						<ItemTemplate>
							<%# Container.DataItem %>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="144px" Height="96px" Description="Vertical - repeatColumn=3">
					<asp:DataList id=DataList2 runat="server" DataSource="<%# m_data %>" RepeatColumns="3">
						<ItemTemplate>
							<%# Container.DataItem %>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Width="144px" Height="96px" Description="Vertical - repeatColumn=10">
					<asp:DataList id=DataList3 runat="server" DataSource="<%# m_data %>" RepeatColumns="10">
						<ItemTemplate>
							<%# Container.DataItem %>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest>&nbsp;</P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Width="144px" Height="96px" Description="Horizontal - repeatColumn=0">
					<asp:DataList id=DataList4 runat="server" DataSource="<%# m_data %>" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%# Container.DataItem %>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest>
				<cc1:GHTSubTest id="GHTSubTest5" runat="server" Width="144px" Height="96px" Description="Horizontal - repeatColumn=3">
					<asp:DataList id=DataList5 runat="server" DataSource="<%# m_data %>" RepeatColumns="3" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%# Container.DataItem %>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest>
				<cc1:GHTSubTest id="GHTSubTest6" runat="server" Width="144px" Height="96px" Description="Horizontal - repeatColumn=10">
					<asp:DataList id=DataList6 runat="server" DataSource="<%# m_data %>" RepeatColumns="10" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%# Container.DataItem %>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
