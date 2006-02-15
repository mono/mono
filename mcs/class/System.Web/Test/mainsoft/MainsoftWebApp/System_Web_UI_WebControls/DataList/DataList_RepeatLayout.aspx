<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_RepeatLayout.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_RepeatLayout" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_RepeatLayout</title>
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
			<P><cc1:ghtsubtest id="GHTSubTest1" runat="server" Description="GHTSubTest1: Table - one column, vertical "
					Width="104px" Height="128px">
					<asp:DataList id=DataList1 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Table" RepeatColumns="1" RepeatDirection="Vertical">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="GHTSubTest2" runat="server" Description="GHTSubTest2: Flow - one column, vertical "
					Width="104px" Height="130px">
					<asp:DataList id=DataList2 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Table" RepeatColumns="1" RepeatDirection="Vertical">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="GHTSubTest3" runat="server" Description="GHTSubTest3: Table - three columns, vertical "
					Width="216px" Height="100px">
					<asp:DataList id=DataList3 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Table" RepeatColumns="3" RepeatDirection="Vertical">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="GHTSubTest4" runat="server" Description="GHTSubTest4: Flow- three columns, vertical "
					Width="120px" Height="130px">
					<asp:DataList id=DataList4 runat="server" Height="75px" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Flow" RepeatColumns="3" RepeatDirection="Vertical">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="GHTSubTest5" runat="server" Description="GHTSubTest5: Table- one row, vertical "
					Width="40px" Height="80px">
					<asp:DataList id=DataList5 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Table" RepeatColumns="10" RepeatDirection="Vertical">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="GHTSubTest6" runat="server" Description="GHTSubTest6: Flow- one row, vertical "
					Width="104px" Height="130px">
					<asp:DataList id=DataList6 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Flow" RepeatColumns="10" RepeatDirection="Vertical">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="Ghtsubtest7" runat="server" Description="Ghtsubtest7: Table - one column, horizontal "
					Width="104px" Height="140px">
					<asp:DataList id=Datalist7 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Table" RepeatColumns="1" RepeatDirection="horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="Ghtsubtest8" runat="server" Description="Ghtsubtest8: Flow - one column, horizontal "
					Width="88px" Height="202px">
					<asp:DataList id=Datalist8 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Flow" RepeatColumns="1" RepeatDirection="horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="Ghtsubtest9" runat="server" Description="Ghtsubtest9: Table - three columns, horizontal "
					Width="216px" Height="90px">
					<asp:DataList id=Datalist9 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Table" RepeatColumns="3" RepeatDirection="horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="Ghtsubtest10" runat="server" Description="Ghtsubtest10: Flow- three columns, horizontal "
					Width="40px" Height="80px">
					<asp:DataList id=Datalist10 runat="server" Height="136px" Width="120px" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Flow" RepeatColumns="3" RepeatDirection="horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="Ghtsubtest11" runat="server" Description="Ghtsubtest11: Table- one row, horizontal "
					Width="360px" Height="102px">
					<asp:DataList id=Datalist11 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Table" RepeatColumns="10" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="Ghtsubtest12" runat="server" Description="Ghtsubtest12: Flow- one row, horizontal "
					Width="120px" Height="138px">
					<asp:DataList id=Datalist12 runat="server" GridLines="Both" BorderWidth="1px" BorderColor="Black" DataSource="<%# m_data %>" RepeatLayout="Flow" RepeatColumns="10" RepeatDirection="Horizontal">
						<ItemTemplate>
							<%#Container.DataItem%>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
			<P>&nbsp;</P>
		</form>
	</body>
</HTML>
