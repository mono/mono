<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_AlternatingItemTemplate.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_AlternatingItemTemplate" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_AlternatingItemTemplate</title>
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
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="104px" Width="176px" Description="Fixed text">
				<asp:DataList id="DataList1" runat="server">
					<AlternatingItemTemplate>
						fixed text
					</AlternatingItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="Ghtsubtest2" runat="server" Height="104px" Width="176px" Description="Data bound">
				<asp:DataList id="Datalist2" runat="server">
					<AlternatingItemTemplate>
						<%#Container.DataItem%>
					</AlternatingItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="Ghtsubtest3" runat="server" Height="104px" Width="176px" Description="Control">
				<asp:DataList id="Datalist3" runat="server">
					<ItemTemplate>
						<asp:TextBox id="TextBox1" runat="server" Text="<%#Container.DataItem%>">
						</asp:TextBox>
					</ItemTemplate>
					<AlternatingItemTemplate>
						<asp:Button id="Button1" runat="server" Text="<%#Container.DataItem%>">
						</asp:Button>
					</AlternatingItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" runat="server" Width="176px" Height="104px" Description="Table">
				<asp:DataList id="DataList4" runat="server">
					<ItemTemplate>
						<tr bgcolor="ActiveCaption">
							<td><%#Container.DataItem%></td>
						</tr>
					</ItemTemplate>
					<AlternatingItemTemplate>
						<tr bgcolor="ThreeDFace">
							<td><%#Container.DataItem%></td>
						</tr>
					</AlternatingItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" runat="server" Width="176px" Height="104px" Description="Template loaded at runtime">
				<asp:DataList id="DataList5" runat="server"></asp:DataList>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
