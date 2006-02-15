<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_SelectedItem.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_SelectedItem" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_SelectedItem</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="96px" Width="144px" Description="None selected."></cc1:GHTSubTest>
			<asp:DataList id=DataList1 runat="server" DataSource="<%# m_data %>" >
				<ItemTemplate>
					<%#Container.DataItem%>
				</ItemTemplate>
				<SelectedItemTemplate>
					Selected Item
				</SelectedItemTemplate>
			</asp:DataList>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="144px" Height="96px" Description="Legal value selected."></cc1:GHTSubTest>
				<asp:DataList id=DataList2 runat="server" DataSource="<%# m_data %>" SelectedIndex="3">
					<SelectedItemTemplate>
						Selected Item
					</SelectedItemTemplate>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList></P>
		</form>
		<P>&nbsp;</P>
		<P>&nbsp;</P>
	</body>
</HTML>
