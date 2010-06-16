<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Prueba._Default" Title="<%$ Resources:Labels, Contact %>" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Bug #6077722 test</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
	<asp:FormView ID="FormView1" runat="server" DataSourceID="ObjectDataSource1" DataKeyNames="M1">
		<EditItemTemplate>
			M1:
			<asp:TextBox ID="M1TextBox" runat="server" Text='<%# Bind("M1") %>' />
			<br />
			M2:
			<asp:TextBox ID="M2TextBox" runat="server" Text='<%# Bind("M2") %>' />
			<br />
			<asp:LinkButton ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" Text="Update" />
			&nbsp;<asp:LinkButton ID="UpdateCancelButton" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" />
		</EditItemTemplate>
		<InsertItemTemplate>
			M1:
			<asp:TextBox ID="M1TextBox" runat="server" Text='<%# Bind("M1") %>' />
			<br />
			M2:
			<asp:TextBox ID="M2TextBox" runat="server" Text='<%# Bind("M2") %>' />
			<br />
			<asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" CommandName="Insert" Text="Insert" />
			&nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" />
		</InsertItemTemplate>
		<ItemTemplate>
			<%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %>M1: <asp:Label ID="M1Label" runat="server" Text='<%# Bind("M1") %>' /><br />M2: <asp:Label ID="M2Label" runat="server" Text='<%# Bind("M2") %>' /><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
			<br />
			<asp:LinkButton ID="EditButton" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit" />
			&nbsp;<asp:LinkButton ID="DeleteButton" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" />
			&nbsp;<asp:LinkButton ID="NewButton" runat="server" CausesValidation="False" CommandName="New" Text="New" />
		</ItemTemplate>
	</asp:FormView>
	<asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataSourceID="ObjectDataSource1" onselectedindexchanged="GridView1_SelectedIndexChanged" DataKeyNames="M1">
		<Columns>
			<asp:CommandField ShowSelectButton="True" />
			<asp:BoundField DataField="M1" HeaderText="M1" SortExpression="M1" />
			<asp:BoundField DataField="M2" HeaderText="M2" SortExpression="M2" />
		</Columns>
	</asp:GridView>
	<asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="Delete" InsertMethod="insert" OldValuesParameterFormatString="old{0}" SelectMethod="Retrieve" 
				TypeName="Prueba.DataSource" UpdateMethod="Update">
		<DeleteParameters>
			<asp:Parameter Name="oldM1" Type="Int32" />
		</DeleteParameters>
		<UpdateParameters>
			<asp:Parameter Name="m1" Type="Int32" />
			<asp:Parameter Name="m2" Type="String" />
			<asp:Parameter Name="oldM1" Type="Int32" />
		</UpdateParameters>
		<InsertParameters>
			<asp:Parameter Name="m1" Type="Int32" />
			<asp:Parameter Name="m2" Type="String" />
		</InsertParameters>
	</asp:ObjectDataSource>
	</div>
    </form>
</body>
</html>
