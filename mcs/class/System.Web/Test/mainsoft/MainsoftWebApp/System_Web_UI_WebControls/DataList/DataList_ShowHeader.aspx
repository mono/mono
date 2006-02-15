<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_ShowHeader.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_ShowHeader" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_ShowHeader</title>
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
		<FORM id="Form1" method="post" runat="server">
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="144px" Height="96px" Description="header template set + Show header enabled.">
				<asp:DataList id=DataList1 runat="server" DataSource="<%# m_data %>">
					<headerTemplate>
						header
					</headerTemplate>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="144px" Height="96px" Description="header template set + Show header disabled.">
				<asp:DataList id=DataList2 runat="server" DataSource="<%# m_data %>" Showheader="False">
					<headerTemplate>
						header
					</headerTemplate>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Width="144px" Height="96px" Description="header template not set + Show header enabled.">
				<asp:DataList id=DataList3 runat="server" DataSource="<%# m_data %>">
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" runat="server" Width="144px" Height="96px" Description="header template not set + Show header disabled.">
				<asp:DataList id=DataList4 runat="server" DataSource="<%# m_data %>" Showheader="False">
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>&nbsp;
		</FORM>
	</body>
</HTML>
