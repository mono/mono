<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_RepeatColumns.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_RepeatColumns" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_RepeatColumns</title>
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
			<P><cc1:ghtsubtest id="GHTSubTest1" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 0 vertical">
					<asp:DataList id=DataList1 runat="server" DataSource="<%# m_data %>">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest2" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 1 vertical">
					<asp:DataList id=DataList2 runat="server" DataSource="<%# m_data %>" RepeatColumns="1">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest3" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 3 vertical">
					<asp:DataList id=DataList3 runat="server" DataSource="<%# m_data %>" RepeatColumns="3">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest6" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 6 vertical">
					<asp:DataList id=DataList6 runat="server" DataSource="<%# m_data %>" RepeatColumns="6">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest4" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 20 vertical">
					<asp:DataList id=DataList4 runat="server" DataSource="<%# m_data %>" RepeatColumns="20">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest>&nbsp;
				<cc1:ghtsubtest id="GHTSubTest5" runat="server" Width="120px" Height="128px" Description="RepeateColumns = -1 vertical">
					<asp:DataList id=DataList5 runat="server" DataSource="<%# m_data %>">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest>&nbsp;</P>
			<P>&nbsp;
				<cc1:ghtsubtest id="GHTSubTest7" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 0 Horizontal">
					<asp:DataList id=DataList7 runat="server" DataSource="<%# m_data %>" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest8" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 0 Horizontal">
					<asp:DataList id=DataList8 runat="server" DataSource="<%# m_data %>" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest9" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 3 Horizontal">
					<asp:DataList id=DataList9 runat="server" DataSource="<%# m_data %>" RepeatColumns="3" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest12" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 6 Horizontal">
					<asp:DataList id=DataList12 runat="server" DataSource="<%# m_data %>" RepeatColumns="6" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest10" runat="server" Width="120px" Height="128px" Description="RepeateColumns = 20 Horizontal">
					<asp:DataList id=DataList10 runat="server" DataSource="<%# m_data %>" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest>
				<cc1:GHTSubTest id="GHTSubTest11" runat="server" Width="120px" Height="128px" Description="RepeateColumns = -1 Horizontal">
					<asp:DataList id=DataList11 runat="server" DataSource="<%# m_data %>" RepeatColumns="20" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
