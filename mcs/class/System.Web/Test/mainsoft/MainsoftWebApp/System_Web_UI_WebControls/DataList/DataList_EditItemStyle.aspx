<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_EditItemStyle.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_EditItemStyle" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_EditItemStyle</title>
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
			<cc1:ghtsubtest id="GHTSubTest1" runat="server" Width="144px" Height="96px" Description="Default">
				<asp:DataList id="DataList1" runat="server">
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:ghtsubtest>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="152px" Height="109px" Description="Set.">
				<asp:DataList id="DataList2" runat="server" EditItemIndex="1">
					<EditItemStyle Font-Names="David" BorderWidth="1px" ForeColor="Maroon" BorderStyle="Solid" BorderColor="Red"
						BackColor="Turquoise"></EditItemStyle>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>&nbsp;
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Width="144px" Height="96px" Description='Overrides "SelectedItemStyle"'>
				<asp:DataList id="DataList3" runat="server" EditItemIndex="2" SelectedIndex="2">
					<SelectedItemStyle Font-Names="Aharoni" BorderWidth="2px" ForeColor="Maroon" BorderStyle="Double" BorderColor="#000040"
						BackColor="#FFC0C0"></SelectedItemStyle>
					<EditItemStyle Font-Names="David" BorderWidth="1px" ForeColor="Maroon" BorderStyle="Solid" BorderColor="Red"
						BackColor="Turquoise"></EditItemStyle>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" runat="server" Width="144px" Height="96px" Description='Overrides "AlternatingItemStyle"'>
				<asp:DataList id="DataList4" runat="server" EditItemIndex="3">
					<EditItemStyle Font-Names="David" BorderWidth="1px" ForeColor="Maroon" BorderStyle="Solid" BorderColor="Red"
						BackColor="Turquoise"></EditItemStyle>
					<AlternatingItemStyle Font-Names="Aharoni" BorderWidth="2px" ForeColor="Maroon" BorderStyle="Double" BorderColor="#000040"
						BackColor="#FFC0C0"></AlternatingItemStyle>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" runat="server" Width="144px" Height="96px" Description='Overrides "ItemStyle"'>
				<asp:DataList id="DataList5" runat="server" EditItemIndex="5">
					<EditItemStyle Font-Names="David" BorderWidth="1px" ForeColor="Maroon" BorderStyle="Solid" BorderColor="Red"
						BackColor="Turquoise"></EditItemStyle>
					<ItemStyle Font-Names="Aharoni" BorderWidth="2px" ForeColor="Maroon" BorderStyle="Double" BorderColor="#000040"
						BackColor="#FFC0C0"></ItemStyle>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" runat="server" Width="144px" Height="96px" Description='Overrides "ControlStyle"'>
				<asp:DataList id="DataList6" runat="server" EditItemIndex="6" BackColor="#FFC0C0" BorderColor="#000040"
					BorderStyle="Double" BorderWidth="2px" Font-Names="Aharoni" ForeColor="Maroon">
					<EditItemStyle Font-Names="David" BorderWidth="1px" ForeColor="Maroon" BorderStyle="Solid" BorderColor="Red"
						BackColor="Turquoise"></EditItemStyle>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest></form>
	</body>
</HTML>
