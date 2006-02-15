<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_GridLines.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_GridLines" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_GridLines</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="96px" Width="504px" Description="None">
					<asp:DataList id=DataList1 runat="server" Width="544px" DataSource="<%# m_data %>" BorderStyle="None">
						<HeaderTemplate>
							<tr style="FONT-WEIGHT: bold; TEXT-TRANSFORM: capitalize; COLOR: navy; TEXT-ALIGN: center; TEXT-DECORATION: underline">
								<TD>StringValue</TD>
								<TD>PriceValue</TD>
								<TD>DescriptionValue</TD>
								<TD>ExponentialValue</TD>
							</tr>
						</HeaderTemplate>
						<ItemTemplate>
							<tr style="TEXT-ALIGN: center;">
								<TD><%# DataBinder.Eval(Container.DataItem, "StringValue")%></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "PriceValue")%></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "DescriptionValue")%></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "ExponentialValue")%></TD>
							</tr>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="144px" Height="96px" Description="Horizontal">
					<asp:DataList id=DataList2 runat="server" Width="552px" DataSource="<%# m_data %>" GridLines="Horizontal" BorderStyle="None">
						<HeaderTemplate>
							<tr style="FONT-WEIGHT: bold; TEXT-TRANSFORM: capitalize; COLOR: navy; TEXT-ALIGN: center; TEXT-DECORATION: underline">
								<TD>StringValue</TD>
								<TD>PriceValue</TD>
								<TD>DescriptionValue</TD>
								<TD>ExponentialValue</TD>
							</tr>
						</HeaderTemplate>
						<ItemTemplate>
							<tr style="TEXT-ALIGN: center;">
								<TD><%# DataBinder.Eval(Container.DataItem, "StringValue") %></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "PriceValue") %></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "DescriptionValue") %></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "ExponentialValue")%></TD>
							</tr>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Width="144px" Height="96px" Description="Vertical">
					<asp:DataList id=DataList3 runat="server" Width="552px" DataSource="<%# m_data %>" GridLines="Vertical" BorderStyle="None">
						<HeaderTemplate>
							<tr style="FONT-WEIGHT: bold; TEXT-TRANSFORM: capitalize; COLOR: navy; TEXT-ALIGN: center; TEXT-DECORATION: underline">
								<TD>StringValue</TD>
								<TD>PriceValue</TD>
								<TD>DescriptionValue</TD>
								<TD>ExponentialValue</TD>
							</tr>
						</HeaderTemplate>
						<ItemTemplate>
							<tr style="TEXT-ALIGN: center;">
								<TD><%# DataBinder.Eval(Container.DataItem, "StringValue") %></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "PriceValue")%></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "DescriptionValue") %></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "ExponentialValue")%></TD>
							</tr>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Width="144px" Height="96px" Description="Both">
					<asp:DataList id=DataList4 runat="server" Width="552px" DataSource="<%# m_data %>" GridLines="Both" BorderStyle="None">
						<HeaderTemplate>
							<tr style="FONT-WEIGHT: bold; TEXT-TRANSFORM: capitalize; COLOR: navy; TEXT-ALIGN: center; TEXT-DECORATION: underline">
								<TD>StringValue</TD>
								<TD>PriceValue</TD>
								<TD>DescriptionValue</TD>
								<TD>ExponentialValue</TD>
							</tr>
						</HeaderTemplate>
						<ItemTemplate>
							<tr style="TEXT-ALIGN: center;">
								<TD><%# DataBinder.Eval(Container.DataItem, "StringValue") %></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "PriceValue")%></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "DescriptionValue")%></TD>
								<TD><%# DataBinder.Eval(Container.DataItem, "ExponentialValue") %></TD>
							</tr>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest5" runat="server" Width="144px" Height="96px" Description="Does not apply if RepeatLayout = flow">
					<asp:DataList id=DataList5 runat="server" Width="552px" DataSource="<%# m_data %>" GridLines="Both" RepeatLayout="Flow" BorderStyle="None">
						<HeaderTemplate>
							<tr style="FONT-WEIGHT: bold; TEXT-TRANSFORM: capitalize; COLOR: navy; TEXT-ALIGN: center; TEXT-DECORATION: underline">
								<TD>
									StringValue
								</TD>
								<TD>
									PriceValue
								</TD>
								<TD>
									DescriptionValue
								</TD>
								<TD>
									ExponentialValue
								</TD>
							</tr>
						</HeaderTemplate>
						<ItemTemplate>
							<tr style="TEXT-ALIGN: center;">
								<TD>
									<%# DataBinder.Eval(Container.DataItem, "StringValue") %>
								</TD>
								<TD>
									<%# DataBinder.Eval(Container.DataItem, "PriceValue") %>
								</TD>
								<TD>
									<%# DataBinder.Eval(Container.DataItem, "DescriptionValue") %>
								</TD>
								<TD>
									<%# DataBinder.Eval(Container.DataItem, "ExponentialValue") %>
								</TD>
							</tr>
						</ItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest>&nbsp;
			</P>
		</form>
	</body>
</HTML>
