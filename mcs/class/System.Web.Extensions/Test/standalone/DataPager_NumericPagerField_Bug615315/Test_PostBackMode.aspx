<%@ Page Title="Bug 615315, PostBack Mode" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" runat="server" 
	contentplaceholderid="ContentPlaceHolder1">
        <asp:ListView ID="ListView1" runat="server" 
		DataSourceID="ObjectDataSource1">
					<ItemTemplate>
						<tr style="">
							<td>
								<asp:Label ID="M1Label" runat="server" Text='<%# Eval("M1") %>' />
							</td>
							<td>
								<asp:Label ID="M2Label" runat="server" Text='<%# Eval("M2") %>' />
							</td>
						</tr>
					</ItemTemplate>
					<AlternatingItemTemplate>
						<tr style="">
							<td>
								<asp:Label ID="M1Label" runat="server" Text='<%# Eval("M1") %>' />
							</td>
							<td>
								<asp:Label ID="M2Label" runat="server" Text='<%# Eval("M2") %>' />
							</td>
						</tr>
					</AlternatingItemTemplate>
					<EmptyDataTemplate>
						<table runat="server" style="">
							<tr>
								<td>
									No data was returned.</td>
							</tr>
						</table>
					</EmptyDataTemplate>
					<InsertItemTemplate>
						<tr style="">
							<td>
								<asp:Button ID="InsertButton" runat="server" CommandName="Insert" 
									Text="Insert" />
								<asp:Button ID="CancelButton" runat="server" CommandName="Cancel" 
									Text="Clear" />
							</td>
							<td>
								<asp:TextBox ID="M1TextBox" runat="server" Text='<%# Bind("M1") %>' />
							</td>
							<td>
								<asp:TextBox ID="M2TextBox" runat="server" Text='<%# Bind("M2") %>' />
							</td>
						</tr>
					</InsertItemTemplate>
					<LayoutTemplate>
						<table runat="server">
							<tr runat="server">
								<td runat="server">
									<table ID="itemPlaceholderContainer" runat="server" border="0" style="">
										<tr runat="server" style="">
											<th runat="server">
												M1</th>
											<th runat="server">
												M2</th>
										</tr>
										<tr ID="itemPlaceholder" runat="server">
										</tr>
									</table>
								</td>
							</tr>
							<tr runat="server">
								<td runat="server" style="">
									<%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><asp:DataPager ID="DataPager1" runat="server" PageSize="10">
										<Fields>
											<asp:NextPreviousPagerField ButtonType="Button" ShowFirstPageButton="True" 
												ShowNextPageButton="False" ShowPreviousPageButton="False" />
											<asp:NumericPagerField />
											<asp:NextPreviousPagerField ButtonType="Button" ShowLastPageButton="True" 
												ShowNextPageButton="False" ShowPreviousPageButton="False" />
										</Fields>
									</asp:DataPager><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
								</td>
							</tr>
						</table>
					</LayoutTemplate>
					<EditItemTemplate>
						<tr style="">
							<td>
								<asp:Button ID="UpdateButton" runat="server" CommandName="Update" 
									Text="Update" />
								<asp:Button ID="CancelButton" runat="server" CommandName="Cancel" 
									Text="Cancel" />
							</td>
							<td>
								<asp:TextBox ID="M1TextBox" runat="server" Text='<%# Bind("M1") %>' />
							</td>
							<td>
								<asp:TextBox ID="M2TextBox" runat="server" Text='<%# Bind("M2") %>' />
							</td>
						</tr>
					</EditItemTemplate>
					<SelectedItemTemplate>
						<tr style="">
							<td>
								<asp:Label ID="M1Label" runat="server" Text='<%# Eval("M1") %>' />
							</td>
							<td>
								<asp:Label ID="M2Label" runat="server" Text='<%# Eval("M2") %>' />
							</td>
						</tr>
					</SelectedItemTemplate>
	</asp:ListView>
	<asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
		OldValuesParameterFormatString="original_{0}" SelectMethod="Retrieve" 
		TypeName="Prueba.DataSource"></asp:ObjectDataSource>


</asp:Content>

