<%@ Page Language="c#" AutoEventWireup="false" Codebehind="BaseDataList_CellPadding.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.BaseDataList_CellPadding" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>BaseDataList_CellPadding</title>
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
			<P><br>
				<cc1:ghtsubtest id="GHTSubTest1" runat="server" DESIGNTIMEDRAGDROP="36" Description="DataGrid_CellPadding1"
					Width="176px" Height="123px">
					<asp:DataGrid id="DataGrid1" runat="server" AutoGenerateColumns="False">
						<Columns>
							<asp:BoundColumn DataField="colA" HeaderText="col a"></asp:BoundColumn>
							<asp:BoundColumn DataField="colB" HeaderText="col b"></asp:BoundColumn>
							<asp:BoundColumn DataField="colC" HeaderText="col C"></asp:BoundColumn>
						</Columns>
					</asp:DataGrid>
				</cc1:ghtsubtest>
				<cc1:ghtsubtest id="GHTSubTest2" tabIndex="5" runat="server" Height="40px" Width="277px" Description="DataGrid_CellPadding3">
					<asp:DataGrid id="DataGrid2" runat="server" AutoGenerateColumns="False" CellPadding="5">
						<Columns>
							<asp:BoundColumn DataField="colA" HeaderText="col a"></asp:BoundColumn>
							<asp:BoundColumn DataField="colB" HeaderText="col b"></asp:BoundColumn>
							<asp:BoundColumn DataField="colC" HeaderText="col C"></asp:BoundColumn>
						</Columns>
					</asp:DataGrid>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest3" runat="server" Description="DataGrid_CellPadding2" Width="225px"
					Height="148px">
					<asp:DataGrid id="DataGrid3" runat="server" AutoGenerateColumns="False" CellPadding="0">
						<Columns>
							<asp:BoundColumn DataField="colA" HeaderText="col a"></asp:BoundColumn>
							<asp:BoundColumn DataField="colB" HeaderText="col b"></asp:BoundColumn>
							<asp:BoundColumn DataField="colC" HeaderText="col C"></asp:BoundColumn>
						</Columns>
					</asp:DataGrid>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest4" tabIndex="5" runat="server" Description="DataGrid_CellPadding4"
					Width="215px" Height="40px">
					<asp:DataGrid id="DataGrid4" runat="server" AutoGenerateColumns="False" CellPadding="0">
						<Columns>
							<asp:BoundColumn DataField="colA" HeaderText="col a"></asp:BoundColumn>
							<asp:BoundColumn DataField="colB" HeaderText="col b"></asp:BoundColumn>
							<asp:BoundColumn DataField="colC" HeaderText="col C"></asp:BoundColumn>
						</Columns>
					</asp:DataGrid>
				</cc1:ghtsubtest></P>
			<P><cc1:ghtsubtest id="GHTSubTest5" runat="server" Width="86px">
					<asp:DataList id="DataList1" runat="server" GridLines="Both">
						<ItemTemplate>
							<td><%# DataBinder.Eval(Container.DataItem,"colA") %></td>
							<td><%# DataBinder.Eval(Container.DataItem,"colB") %></td>
							</asp:TextBox>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest6" runat="server" Width="86px">
					<asp:DataList id="DataList2" runat="server" GridLines="Both">
						<ItemTemplate>
							<td><%# DataBinder.Eval(Container.DataItem,"colA") %></td>
							<td><%# DataBinder.Eval(Container.DataItem,"colB") %></td>
							</asp:TextBox>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest7" runat="server" Width="103px">
					<asp:DataList id="DataList3" runat="server" DESIGNTIMEDRAGDROP="258" GridLines="Both">
						<ItemTemplate>
							<td><%# DataBinder.Eval(Container.DataItem,"colA") %></td>
							<td><%# DataBinder.Eval(Container.DataItem,"colB") %></td>
							</asp:TextBox>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest><cc1:ghtsubtest id="GHTSubTest8" runat="server" Width="93px">
					<asp:DataList id="DataList4" runat="server" GridLines="Both">
						<ItemTemplate>
							<td><%# DataBinder.Eval(Container.DataItem,"colA") %></td>
							<td><%# DataBinder.Eval(Container.DataItem,"colB") %></td>
							</asp:TextBox>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
		</form>
	</body>
</HTML>
