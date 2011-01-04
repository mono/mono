<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
        SelectMethod="GetCountries" TypeName="TestCode.CountryCollection" 
        SortParameterName="sortExpression" UpdateMethod="Update" 
        DeleteMethod="Delete" InsertMethod="Insert">
        <DeleteParameters>
            <asp:Parameter Name="id" Type="Int32" />
        </DeleteParameters>
        <UpdateParameters>
            <asp:Parameter Name="id" Type="Int32" />
            <asp:Parameter Name="name" Type="String" />
            <asp:Parameter Name="capital" Type="String" />
            <asp:Parameter Name="population" Type="Double" />
        </UpdateParameters>
        <SelectParameters>
            <asp:Parameter Name="sortExpression" Type="String" />
        </SelectParameters>
        <InsertParameters>
            <asp:Parameter Name="id" Type="Int32" />
            <asp:Parameter Name="name" Type="String" />
            <asp:Parameter Name="capital" Type="String" />
            <asp:Parameter Name="population" Type="Double" />
        </InsertParameters>
    </asp:ObjectDataSource>
    
    <div>
        <t:ListViewPoker ID="ListView1" runat="server" DataSourceID="ObjectDataSource1" InsertItemPosition="LastItem">
            <ItemTemplate>
                <tr style="">
                    <td>
                        <asp:Button ID="DeleteButton" runat="server" CommandName="Delete" 
                            Text="Delete" />
                        <asp:Button ID="EditButton" runat="server" CommandName="Edit" Text="Edit" />
                        <asp:Button ID="SelectButton" runat="server" CommandName="Select" Text="Select" />
                    </td>
                    <td>
                        <asp:Label ID="IDLabel" runat="server" Text='<%# Eval("ID") %>' />
                    </td>
                    <td>
                        <asp:Label ID="NameLabel" runat="server" Text='<%# Eval("Name") %>' />
                    </td>
                    <td>
                        <asp:Label ID="CapitalLabel" runat="server" Text='<%# Eval("Capital") %>' />
                    </td>
                    <td>
                        <asp:Label ID="PopulationLabel" runat="server" 
                            Text='<%# Eval("Population") %>' />
                    </td>
                </tr>
            </ItemTemplate>
            <AlternatingItemTemplate>
                <tr style="">
                    <td>
                        <asp:Button ID="DeleteButton" runat="server" CommandName="Delete" 
                            Text="Delete" />
                        <asp:Button ID="EditButton" runat="server" CommandName="Edit" Text="Edit" />
                        <asp:Button ID="SelectButton" runat="server" CommandName="Select" Text="Select" />
                    </td>
                    <td>
                        <asp:Label ID="IDLabel" runat="server" Text='<%# Eval("ID") %>' />
                    </td>
                    <td>
                        <asp:Label ID="NameLabel" runat="server" Text='<%# Eval("Name") %>' />
                    </td>
                    <td>
                        <asp:Label ID="CapitalLabel" runat="server" Text='<%# Eval("Capital") %>' />
                    </td>
                    <td>
                        <asp:Label ID="PopulationLabel" runat="server" 
                            Text='<%# Eval("Population") %>' />
                    </td>
                </tr>
            </AlternatingItemTemplate>
            <EmptyDataTemplate>
                <table runat="server" 
                    
                    style="">
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
                        <asp:TextBox ID="IDTextBox" runat="server" Text='<%# Bind("ID") %>' />
                    </td>
                    <td>
                        <asp:TextBox ID="NameTextBox" runat="server" Text='<%# Bind("Name") %>' />
                    </td>
                    <td>
                        <asp:TextBox ID="CapitalTextBox" runat="server" Text='<%# Bind("Capital") %>' />
                    </td>
                    <td>
                        <asp:TextBox ID="PopulationTextBox" runat="server" 
                            Text='<%# Bind("Population") %>' />
                    </td>
                </tr>
            </InsertItemTemplate>
            <LayoutTemplate>
                <table runat="server">
                    <tr runat="server">
                        <td runat="server">
                            <table ID="itemPlaceholderContainer" runat="server" border="0" 
                                style="">
                                <tr runat="server" style="">
                                    <th runat="server">
                                    </th>
                                    <th runat="server">
                                        ID</th>
                                    <th runat="server">
                                        Name</th>
                                    
                                    <th runat="server">
                                        Capital</th>
                                    <th runat="server">
                                        Population</th>
                                    
                                </tr>
                                <tr ID="itemPlaceholder" runat="server">
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr runat="server">
                        <td runat="server" 
                            style="">
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
                        <asp:TextBox ID="IDTextBox" runat="server" Text='<%# Bind("ID") %>' />
                    </td>
                    <td>
                        <asp:TextBox ID="NameTextBox" runat="server" Text='<%# Bind("Name") %>' />
                    </td>
                    <td>
                        <asp:TextBox ID="CapitalTextBox" runat="server" Text='<%# Bind("Capital") %>' />
                    </td>
                    <td>
                        <asp:TextBox ID="PopulationTextBox" runat="server" 
                            Text='<%# Bind("Population") %>' />
                    </td>
                </tr>
            </EditItemTemplate>
            <SelectedItemTemplate>
                <tr style="">
                    <td>
                        <asp:Button ID="DeleteButton" runat="server" CommandName="Delete" 
                            Text="Delete" />
                        <asp:Button ID="EditButton" runat="server" CommandName="Edit" Text="Edit" />
                    </td>
                    <td>
                        <asp:Label ID="IDLabel" runat="server" Text='<%# Eval("ID") %>' />
                    </td>
                    <td>
                        <asp:Label ID="NameLabel" runat="server" Text='<%# Eval("Name") %>' />
                    </td>
                    <td>
                        <asp:Label ID="CapitalLabel" runat="server" Text='<%# Eval("Capital") %>' />
                    </td>
                    <td>
                        <asp:Label ID="PopulationLabel" runat="server" 
                            Text='<%# Eval("Population") %>' />
                    </td>
                </tr>
            </SelectedItemTemplate>
        </t:ListViewPoker>
    </div>
    </form>
</body>
</html>
