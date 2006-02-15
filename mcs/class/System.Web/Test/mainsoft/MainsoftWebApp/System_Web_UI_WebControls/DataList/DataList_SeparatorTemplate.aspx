<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_SeparatorTemplate.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_SeparatorTemplate" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_SeparatorTemplate</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="176px" Height="104px" Description="Fixed text">
				<asp:DataList id=DataList1 runat="server" DataSource="<%# m_data %>">
					<separatorTemplate>
						fixed text
					</separatorTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="176px" Height="104px" Description="Data Bound">
				<asp:DataList id=DataList2 runat="server" DataSource="<%# m_data %>">
					<separatorTemplate>
						<%#Container.DataItem%>
					</separatorTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="Ghtsubtest3" runat="server" Width="176px" Height="104px" Description="Control">
				<asp:DataList id=Datalist3 runat="server" DataSource="<%# m_data %>">
					<ItemTemplate>
						<asp:TextBox id="TextBox1" runat="server" Text="<%#Container.DataItem%>">
						</asp:TextBox>
					</ItemTemplate>
					<separatorTemplate>
						<asp:Button id="Button1" runat="server" Text="separator"></asp:Button>
					</separatorTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" runat="server" Width="176px" Height="104px" Description="Table">
				<asp:DataList id=DataList4 runat="server" DataSource="<%# m_data %>">
					<ItemTemplate>
						<tr bgcolor="ActiveCaption">
							<td><%#Container.DataItem%></td>
						</tr>
					</ItemTemplate>
					<separatorTemplate>
						<tr bgcolor="ThreeDFace">
							<td>separator</td>
							<td>separator</td>
						</tr>
					</separatorTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" runat="server" Width="176px" Height="104px" Description="Template loaded at runtime">
				<asp:DataList id=DataList5 runat="server" DataSource="<%# m_data %>">
				</asp:DataList>
			</cc1:GHTSubTest></FORM>
	</body>
</HTML>
