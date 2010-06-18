<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
			<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:ListView ID="Bug604053ListView1" runat="server" DataSourceID="ObjectDataSource1">
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
								<asp:DataPager ID="DataPager1" runat="server" PageSize="2">
									<Fields>
										<asp:NextPreviousPagerField ButtonType="Button" ShowFirstPageButton="True" 
											ShowNextPageButton="False" ShowPreviousPageButton="False" />
										<asp:NumericPagerField />
										<asp:NextPreviousPagerField ButtonType="Button" ShowLastPageButton="True" 
											ShowNextPageButton="False" ShowPreviousPageButton="False" />
									</Fields>
								</asp:DataPager>
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
			<asp:ListView ID="Bug604053ListView2" runat="server" DataSourceID="ObjectDataSource1">
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
								<asp:DataPager ID="DataPager1" runat="server" PageSize="2">
									<Fields>
										<asp:NextPreviousPagerField ButtonType="Button" ShowFirstPageButton="True" 
											ShowLastPageButton="True" />
									</Fields>
								</asp:DataPager>
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
			</asp:ListView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
			<asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
				OldValuesParameterFormatString="original_{0}" SelectMethod="Retrieve" 
				TypeName="Bug604053.Prueba.DataSource"></asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
