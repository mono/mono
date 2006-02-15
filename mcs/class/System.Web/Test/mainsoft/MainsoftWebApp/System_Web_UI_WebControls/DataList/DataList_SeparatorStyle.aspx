<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_SeparatorStyle.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_SeparatorStyle" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_SeparatorStyle</title>
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
		<FORM id="Form2" method="post" runat="server">
			<cc1:ghtsubtest id="GHTSubTest1" runat="server" Width="104px" Height="96px" Description="Default">
				<asp:DataList id=DataList1 runat="server" DataSource="<%# m_data %>">
					<SeparatorTemplate>
						separator
					</SeparatorTemplate>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest2" runat="server" Width="104px" Height="96px" Description="Set">
				<asp:DataList id=DataList2 runat="server" DataSource="<%# m_data %>">
					<separatorTemplate>
						<P>separator</P>
					</separatorTemplate>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
					<separatorStyle Font-Overline="True" Font-Underline="True" Font-Names="Arial Narrow" Font-Italic="True"
						Font-Bold="True" Font-Strikeout="True" BorderWidth="3px" ForeColor="#004000" BorderStyle="Double"
						BorderColor="Lime" BackColor="Purple"></separatorStyle>
				</asp:DataList>
			</cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest3" runat="server" Width="104px" Height="96px" Description="Inherits control style">
				<asp:DataList id=DataList3 runat="server" DataSource="<%# m_data %>" BackColor="#FF8080" BorderColor="#FFFFC0" BorderStyle="Groove" Font-Names="AngsanaUPC" ForeColor="DarkRed" GridLines="Both">
					<separatorTemplate>
						separator
					</separatorTemplate>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest4" runat="server" Width="104px" Height="96px" Description="Overrides control style">
				<asp:DataList id=DataList4 runat="server" DataSource="<%# m_data %>" BackColor="#FF8080" BorderColor="#FFFFC0" BorderStyle="Groove" Font-Names="AngsanaUPC" ForeColor="DarkRed" GridLines="Both">
					<separatorTemplate>
						separator
					</separatorTemplate>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
					<separatorStyle Font-Names="Batang" ForeColor="Purple" BorderStyle="Dotted" BorderColor="#000040"
						BackColor="#8080FF"></separatorStyle>
				</asp:DataList>
			</cc1:ghtsubtest>&nbsp;</FORM>
	</body>
</HTML>
