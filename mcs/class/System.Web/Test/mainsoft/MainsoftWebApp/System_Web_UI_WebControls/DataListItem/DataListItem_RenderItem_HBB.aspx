<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataListItem_RenderItem_HBB.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataListItem_RenderItem_HBB" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataListItem_RenderItem_HBB</title>
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
			<asp:datalist id=DataList1 runat="server" EditItemIndex="2" SelectedIndex="3" DataSource="<%# m_data %>" >
				<HeaderTemplate>
					<asp:Table id="Table1" runat="server">
						<asp:TableRow>
							<asp:tablecell>
								<span>Header</span></asp:tablecell>
						</asp:TableRow>
					</asp:Table>
				</HeaderTemplate>
				<SelectedItemTemplate>
					<asp:Table id="Table2" runat="server">
						<asp:TableRow>
							<asp:tablecell>
								<span>
									<%#Container.DataItem%>
								</span>
							</asp:tablecell>
						</asp:TableRow>
					</asp:Table>
				</SelectedItemTemplate>
				<FooterTemplate>
					<asp:Table id="Table3" runat="server">
						<asp:TableRow>
							<asp:tablecell>
								<span>Footer</span></asp:tablecell>
						</asp:TableRow>
					</asp:Table>
				</FooterTemplate>
				<ItemTemplate>
					<asp:Table id="Table4" runat="server">
						<asp:TableRow>
							<asp:tablecell>
								<span>
									<%#Container.DataItem%>
								</span>
							</asp:tablecell>
						</asp:TableRow>
					</asp:Table>
				</ItemTemplate>
				<SeparatorTemplate>
					<asp:Table id="Table5" runat="server">
						<asp:TableRow>
							<asp:tablecell>
								<span>Seperator</span></asp:tablecell>
						</asp:TableRow>
					</asp:Table>
				</SeparatorTemplate>
				<AlternatingItemTemplate>
					<asp:Table id="Table6" runat="server">
						<asp:TableRow>
							<asp:tablecell>
								<span>
									<%#Container.DataItem%>
								</span>
							</asp:tablecell>
						</asp:TableRow>
					</asp:Table>
				</AlternatingItemTemplate>
				<EditItemTemplate>
					<asp:Table id="Table7" runat="server">
						<asp:TableRow>
							<asp:tablecell>
								<span>
									<%#Container.DataItem%>
								</span>
							</asp:tablecell>
						</asp:TableRow>
					</asp:Table>
				</EditItemTemplate>
			</asp:datalist>
			<P>
				<cc1:ghtsubtest id="GHTSubTest1" runat="server" Width="144px" Height="96px" Description="AlternatingItem Extract = true, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest2" runat="server" Width="144px" Height="96px" Description="AlternatingItem Extract = false, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest3" runat="server" Width="144px" Height="96px" Description="AlternatingItem Extract = true, layout = flow"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest4" runat="server" Width="144px" Height="96px" Description="AlternatingItem Extract = false, layout = flow"></cc1:ghtsubtest></P>
			<P>
				<cc1:ghtsubtest id="Ghtsubtest5" runat="server" Width="144px" Height="96px" Description="EditItem Extract = true, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest6" runat="server" Width="144px" Height="96px" Description="EditItem Extract = false, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest7" runat="server" Width="144px" Height="96px" Description="EditItem Extract = true, layout = flow"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest8" runat="server" Width="144px" Height="96px" Description="EditItem Extract = false, layout = flow"></cc1:ghtsubtest></P>
			<P>
				<cc1:ghtsubtest id="Ghtsubtest9" runat="server" Width="144px" Height="96px" Description="FooterItem Extract = true, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest10" runat="server" Width="144px" Height="96px" Description="FooterItem Extract = false, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest11" runat="server" Width="144px" Height="96px" Description="FooterItem Extract = true, layout = flow"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest12" runat="server" Width="144px" Height="96px" Description="FooterItem Extract = false, layout = flow"></cc1:ghtsubtest></P>
			<P>
				<cc1:ghtsubtest id="Ghtsubtest13" runat="server" Width="144px" Height="96px" Description="Header Extract = true, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest14" runat="server" Width="144px" Height="96px" Description="Header Extract = false, layout = table "></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest15" runat="server" Width="144px" Height="96px" Description="Header Extract = true, layout = flow "></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest16" runat="server" Width="144px" Height="96px" Description="Header  Extract = false, layout = flow"></cc1:ghtsubtest></P>
			<P>
				<cc1:ghtsubtest id="Ghtsubtest17" runat="server" Width="144px" Height="96px" Description="Item Extract = true, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest18" runat="server" Width="144px" Height="96px" Description="Item Extract = false, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest19" runat="server" Width="144px" Height="96px" Description="Item Extract = true, layout = flow"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest20" runat="server" Width="144px" Height="96px" Description="Item Extract = false, layout = flow"></cc1:ghtsubtest></P>
			<P>
				<cc1:ghtsubtest id="Ghtsubtest21" runat="server" Width="144px" Height="96px" Description="Pager Extract = true, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest22" runat="server" Width="144px" Height="96px" Description="Pager Extract = false, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest23" runat="server" Width="144px" Height="96px" Description="Pager Extract = true, layout = flow"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest24" runat="server" Width="144px" Height="96px" Description="Pager Extract = false, layout = flow"></cc1:ghtsubtest></P>
			<P>
				<cc1:ghtsubtest id="Ghtsubtest25" runat="server" Width="144px" Height="96px" Description="SelectedItem Extract = true, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest26" runat="server" Width="144px" Height="96px" Description="SelectedItem Extract = false, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest27" runat="server" Width="144px" Height="96px" Description="SelectedItem Extract = true, layout = flow"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest28" runat="server" Width="144px" Height="96px" Description="SelectedItem Extract = false, layout = flow"></cc1:ghtsubtest></P>
			<P>
				<cc1:ghtsubtest id="Ghtsubtest29" runat="server" Width="144px" Height="96px" Description="Separator Extract = true, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest30" runat="server" Width="144px" Height="96px" Description="Separator Extract = false, layout = table"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest31" runat="server" Width="144px" Height="96px" Description="Separator Extract = true, layout = flow"></cc1:ghtsubtest>
				<cc1:ghtsubtest id="Ghtsubtest32" runat="server" Width="144px" Height="96px" Description="Separator Extract = false, layout = flow"></cc1:ghtsubtest></P>
		</form>
	</body>
</HTML>
