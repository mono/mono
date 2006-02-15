<%@ Page Language="c#" AutoEventWireup="false" Codebehind="BaseDataList_CellSpacing.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.BaseDataList_CellSpacing" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>BaseDataList_CellSpacing</title>
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
			<P><BR>
				<cc1:ghtsubtest id="GHTSubTest1" runat="server" Height="123px" Width="176px" Description="DataGrid_CellSpacing1"
					DESIGNTIMEDRAGDROP="36">
					<asp:DataGrid id="DataGrid1" runat="server" AutoGenerateColumns="False">
						<Columns>
							<asp:BoundColumn DataField="colA" HeaderText="col a"></asp:BoundColumn>
							<asp:BoundColumn DataField="colB" HeaderText="col b"></asp:BoundColumn>
							<asp:BoundColumn DataField="colC" HeaderText="col C"></asp:BoundColumn>
						</Columns>
					</asp:DataGrid>
				</cc1:ghtsubtest>
				<cc1:ghtsubtest id="GHTSubTest2" tabIndex="5" runat="server" Height="40px" Width="277px" Description="DataGrid_CellSpacing2">
					<asp:DataGrid id="DataGrid2" runat="server" AutoGenerateColumns="False" CellPadding="5">
						<Columns>
							<asp:BoundColumn DataField="colA" HeaderText="col a"></asp:BoundColumn>
							<asp:BoundColumn DataField="colB" HeaderText="col b"></asp:BoundColumn>
							<asp:BoundColumn DataField="colC" HeaderText="col C"></asp:BoundColumn>
						</Columns>
					</asp:DataGrid>
				</cc1:ghtsubtest>
				<cc1:ghtsubtest id="GHTSubTest3" runat="server" Height="148px" Width="225px" Description="DataGrid_CellSpacing32">
					<asp:DataGrid id="DataGrid3" runat="server" AutoGenerateColumns="False" CellPadding="0">
						<Columns>
							<asp:BoundColumn DataField="colA" HeaderText="col a"></asp:BoundColumn>
							<asp:BoundColumn DataField="colB" HeaderText="col b"></asp:BoundColumn>
							<asp:BoundColumn DataField="colC" HeaderText="col C"></asp:BoundColumn>
						</Columns>
					</asp:DataGrid>
				</cc1:ghtsubtest>
				<cc1:ghtsubtest id="GHTSubTest4" tabIndex="5" runat="server" Height="40px" Width="215px" Description="DataGrid_CellSpacing4">
					<asp:DataGrid id="DataGrid4" runat="server" AutoGenerateColumns="False" CellPadding="0">
						<Columns>
							<asp:BoundColumn DataField="colA" HeaderText="col a"></asp:BoundColumn>
							<asp:BoundColumn DataField="colB" HeaderText="col b"></asp:BoundColumn>
							<asp:BoundColumn DataField="colC" HeaderText="col C"></asp:BoundColumn>
						</Columns>
					</asp:DataGrid>
				</cc1:ghtsubtest></P>
			<P>
				<cc1:ghtsubtest id="GHTSubTest5" runat="server" Width="86px" Description="DataList_CellSpacing1">
					<asp:DataList id="DataList1" runat="server" GridLines="Both">
						<ItemTemplate>
							<asp:TextBox id=TextBox1 runat="server" Width="39px" Text='<%# DataBinder.Eval(Container.DataItem,"colA") %>'>
							</asp:TextBox>
							<asp:TextBox id=TextBox2 runat="server" Width="31px" Text='<%# DataBinder.Eval(Container.DataItem,"colB") %>'>
							</asp:TextBox>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest>
				<cc1:ghtsubtest id="GHTSubTest6" runat="server" Width="86px" Description="DataList_CellSpacing2">
					<asp:DataList id="DataList2" runat="server" GridLines="Both">
						<ItemTemplate>
							<asp:TextBox id=TextBox1 runat="server" Width="39px" Text='<%# DataBinder.Eval(Container.DataItem,"colA") %>'>
							</asp:TextBox>
							<asp:TextBox id=TextBox2 runat="server" Width="31px" Text='<%# DataBinder.Eval(Container.DataItem,"colB") %>'>
							</asp:TextBox>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest>
				<cc1:ghtsubtest id="GHTSubTest7" runat="server" Width="103px" Description="DataList_CellSpacing3">
					<asp:DataList id="DataList3" runat="server" DESIGNTIMEDRAGDROP="258" GridLines="Both">
						<ItemTemplate>
							<asp:TextBox id=TextBox1 runat="server" Width="39px" Text='<%# DataBinder.Eval(Container.DataItem,"colA") %>'>
							</asp:TextBox>
							<asp:TextBox id=TextBox2 runat="server" Width="31px" Text='<%# DataBinder.Eval(Container.DataItem,"colB") %>'>
							</asp:TextBox>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest>
				<cc1:ghtsubtest id="GHTSubTest8" runat="server" Width="93px" Description="DataList_CellSpacing4">
					<asp:DataList id="DataList4" runat="server" GridLines="Both">
						<ItemTemplate>
							<asp:TextBox id=TextBox1 runat="server" Width="39px" Text='<%# DataBinder.Eval(Container.DataItem,"colA") %>'>
							</asp:TextBox>
							<asp:TextBox id=TextBox2 runat="server" Width="31px" Text='<%# DataBinder.Eval(Container.DataItem,"colB") %>'>
							</asp:TextBox>
						</ItemTemplate>
					</asp:DataList>
				</cc1:ghtsubtest></P>
		</FORM>
		<br>
		<br>
	</body>
</HTML>
