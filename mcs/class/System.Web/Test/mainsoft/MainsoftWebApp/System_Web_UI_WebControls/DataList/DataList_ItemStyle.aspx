<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_ItemStyle.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_ItemStyle" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_ItemStyle</title>
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
			<cc1:ghtsubtest id="GHTSubTest1" runat="server" Width="144px" Height="96px" Description="Default">
				<asp:DataList id=DataList1 runat="server" DataSource="<%# m_data %>">
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:ghtsubtest>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="152px" Height="109px" Description="Set.">
				<asp:DataList id=DataList2 runat="server" EditItemIndex="1" DataSource="<%# m_data %>">
					<itemstyle Font-Names="David" BorderWidth="1px" ForeColor="Maroon" BorderStyle="Solid" BorderColor="Red"
						BackColor="Turquoise"></itemstyle>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>&nbsp;
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Width="144px" Height="96px" Description='Overrides "ControlStyle"'>
				<asp:DataList id=DataList3 runat="server" EditItemIndex="6" BackColor="#FFC0C0" BorderColor="#000040" BorderStyle="Double" BorderWidth="2px" Font-Names="Aharoni" ForeColor="Maroon" DataSource="<%# m_data %>">
					<ItemStyle Font-Names="David" BorderWidth="1px" ForeColor="Maroon" BorderStyle="Solid" BorderColor="Red"
						BackColor="Turquoise"></ItemStyle>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
		</FORM>
	</body>
</HTML>
