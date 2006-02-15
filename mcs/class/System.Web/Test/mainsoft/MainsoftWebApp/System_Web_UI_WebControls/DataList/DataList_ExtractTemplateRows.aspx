<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_ExtractTemplateRows.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_ExtractTemplateRows" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_ExtractTemplateRows</title>
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
			<P>
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="310px" Width="280px" Description="Default value = false">
					<asp:DataList id=DataList1 runat="server" GridLines="Both" DataSource="<%# m_data %>" DataMember="Items" BorderColor="Black" BorderStyle="Solid">
						<AlternatingItemStyle BackColor="Gray"></AlternatingItemStyle>
						<ItemTemplate>
							<asp:Table id="Table1" runat="server" GridLines="Both" BackColor="Blue">
								<asp:TableRow>
									<asp:TableCell>
										<%# DataBinder.Eval(Container.DataItem, "StringValue") %>
									</asp:TableCell>
									<asp:TableCell>
										<%# DataBinder.Eval(Container.DataItem, "PriceValue") %>
									</asp:TableCell>
									<asp:TableCell RowSpan="2">
										<%# DataBinder.Eval(Container.DataItem, "DescriptionValue") %>
									</asp:TableCell>
								</asp:TableRow>
								<asp:TableRow>
									<asp:TableCell ColumnSpan="2">
										<%# DataBinder.Eval(Container.DataItem, "ExponentialValue") %>
									</asp:TableCell>
								</asp:TableRow>
							</asp:Table>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="280px" Height="250px" Description="Set to true">
					<asp:DataList id=DataList2 runat="server" GridLines="Both" DataSource="<%# m_data %>" DataMember="Items" BorderColor="Black" BorderStyle="Solid" ExtractTemplateRows="True">
						<AlternatingItemStyle BackColor="Gray"></AlternatingItemStyle>
						<ItemTemplate>
							<asp:Table id="Table2" runat="server" GridLines="Both">
								<asp:TableRow>
									<asp:TableCell>
										<%# DataBinder.Eval(Container.DataItem, "StringValue") %>
									</asp:TableCell>
									<asp:TableCell>
										<%# DataBinder.Eval(Container.DataItem, "PriceValue") %>
									</asp:TableCell>
									<asp:TableCell RowSpan="2">
										<%# DataBinder.Eval(Container.DataItem, "DescriptionValue") %>
									</asp:TableCell>
								</asp:TableRow>
								<asp:TableRow>
									<asp:TableCell ColumnSpan="2">
										<%# DataBinder.Eval(Container.DataItem, "ExponentialValue") %>
									</asp:TableCell>
								</asp:TableRow>
							</asp:Table>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Width="280px" Height="256px" Description="Show only the table in the template, when set to 'True'">
					<asp:DataList id=DataList3 runat="server" GridLines="Both" DataSource="<%# m_data %>" DataMember="Items" BorderColor="Black" BorderStyle="Solid" ExtractTemplateRows="True">
						<AlternatingItemStyle BackColor="Gray"></AlternatingItemStyle>
						<ItemTemplate>
							<asp:Table id="Table2" runat="server" GridLines="Both">
								<asp:TableRow>
									<asp:TableCell>
										<%# DataBinder.Eval(Container.DataItem, "StringValue") %>
									</asp:TableCell>
									<asp:TableCell>
										<%# DataBinder.Eval(Container.DataItem, "PriceValue") %>
									</asp:TableCell>
									<asp:TableCell RowSpan="2">
										<%# DataBinder.Eval(Container.DataItem, "DescriptionValue") %>
									</asp:TableCell>
								</asp:TableRow>
								<asp:TableRow>
									<asp:TableCell ColumnSpan="2">
										<%# DataBinder.Eval(Container.DataItem, "ExponentialValue") %>
									</asp:TableCell>
								</asp:TableRow>
							</asp:Table>
							<P>
								<asp:Button id="Button1" runat="server" Text="Button"></asp:Button>
								<asp:Image id="Image1" runat="server"></asp:Image>Palin Text</P>
							<DIV style="DISPLAY: inline; WIDTH: 146px; HEIGHT: 26px" ms_positioning="FlowLayout"><STRONG><EM><U><FONT color="#ff0000">Html 
												formatted text</FONT></U></EM></STRONG></DIV>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Width="120px" Height="710px" Description="Show the whole template when set to 'false'">
					<asp:DataList id=DataList4 runat="server" GridLines="Both" DataSource="<%# m_data %>" DataMember="Items" BorderColor="Black" BorderStyle="Solid">
						<AlternatingItemStyle BackColor="Gray"></AlternatingItemStyle>
						<ItemTemplate>
							<asp:Table id="Table2" runat="server" GridLines="Both">
								<asp:TableRow>
									<asp:TableCell>
										<%# DataBinder.Eval(Container.DataItem, "StringValue") %>
									</asp:TableCell>
									<asp:TableCell>
										<%# DataBinder.Eval(Container.DataItem, "PriceValue") %>
									</asp:TableCell>
									<asp:TableCell RowSpan="2">
										<%# DataBinder.Eval(Container.DataItem, "DescriptionValue") %>
									</asp:TableCell>
								</asp:TableRow>
								<asp:TableRow>
									<asp:TableCell ColumnSpan="2">
										<%# DataBinder.Eval(Container.DataItem, "ExponentialValue") %>
									</asp:TableCell>
								</asp:TableRow>
							</asp:Table>
							<P>
								<asp:Button id="Button1" runat="server" Text="Button"></asp:Button>
								<asp:Image id="Image1" runat="server"></asp:Image>Palin Text</P>
							<DIV style="DISPLAY: inline; WIDTH: 146px; HEIGHT: 26px" ms_positioning="FlowLayout"><STRONG><EM><U><FONT color="#ff0000">Html 
												formatted text</FONT></U></EM></STRONG></DIV>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
