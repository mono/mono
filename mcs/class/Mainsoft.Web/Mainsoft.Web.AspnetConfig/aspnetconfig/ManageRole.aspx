<%--
// Mainsoft.Web.AspnetConfig - Site administration utility
// Authors:
//  Klain Yoni <yonik@mainsoft.com>
//
// Mainsoft.Web.AspnetConfig - Site administration utility
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. --%>
<%@ Page Language="C#" AutoEventWireup="true" EnableTheming="false" MasterPageFile="~/aspnetconfig/Util.Master" CodeBehind="ManageRole.aspx.cs" Inherits="Mainsoft.Web.AspnetConfig.MenageRole" %>
<%@ Register tagprefix="custom" namespace="Mainsoft.Web.AspnetConfig" Assembly="Mainsoft.Web.AspnetConfig"  %>
<asp:Content ID="Content1" ContentPlaceHolderID="SiteMapLinks" runat="server">
<td><a class="SiteMap" href="Default.aspx" title="Home page" runat="server">Home</a></td>
<td> >> </td>
<td style="color: #284E98;"><a class="SiteMap" href="CreateRole.aspx" title="Create\Manage roles" runat="server">Create\Manage roles</a></td>
<td> >> </td>
<td style="color: #333333">Manage roles</td>
</asp:Content>

<asp:Content runat="server" ID="main" ContentPlaceHolderID="Main">
    <table class="maintable">
        <tr>
            <td colspan="2" width="100%" style="height: 21px">
                Use this page to manage the members in the specified role. To add a user to the role, search for the user name and then select the User Is In Role check box for that user. 
                <br /> 
                Role:
                <asp:Label   ID="role_lbl" runat="server" /> <br /> 
            </td>
        </tr>
       <tr>
            <td width= "100%" valign="top">
                <table width="100%" class="innertable" >
                    <tr>
                        <td width="100%" class="controlheader">
                            Search for Users
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Search for Users &nbsp; &nbsp; 
                            <asp:DropDownList ID="searchBy_dl" runat="server">
                                <asp:ListItem Selected="True" Value="Name">User Name</asp:ListItem>
                                <asp:ListItem Value="Mail">E-Mail</asp:ListItem> 
                            </asp:DropDownList> &nbsp; &nbsp;<asp:TextBox ID="user_txt" runat="server"></asp:TextBox>
                            &nbsp;
                            <asp:Button ID="searchUser_bt" runat="server" Text="Search" /> <br />
                            Wildcard characters * and ? are permitted. <br /><br />
                        </td>
                    </tr>
                    <tr>
                        <td width= "100%" valign="top" datakeynames="Role;">
                            <asp:GridView ID="Roles_gv" runat="server" CellPadding="4" DataSourceID="ObjectDataSource1" ForeColor="#333333" GridLines="None" Width="100%" AutoGenerateColumns="False" AllowPaging="True" PageSize="5">
                                <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                <Columns>
                                    <asp:BoundField DataField="User" HeaderText="User Name" />
                                    <asp:TemplateField HeaderText="User Is In Role">
                                        <ItemTemplate>
                                            <custom:GridCheckBox ID="Check" runat="server" AutoPostBack="true" Checked='<%# Bind("InRole") %>'
                                             User='<%# Bind("User") %>' OnCheckedChanged="CheckBox_CheckedChanged" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <RowStyle BackColor="#EFF3FB" />
                                <EditRowStyle BackColor="#2461BF" />
                                <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
                                <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
                                <HeaderStyle BackColor="#5D7B9D" BorderStyle="Solid" Font-Bold="True" Font-Size="0.9em"
                                    ForeColor="White" HorizontalAlign="Left" />
                                <AlternatingRowStyle BackColor="White" />
                            </asp:GridView>
                            <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="Delete"
                                SelectMethod="CreateManageRoleTable" TypeName="Mainsoft.Web.AspnetConfig.RolesDS">
                                <DeleteParameters>
                                    <asp:Parameter Name="Role" Type="String" />
                                </DeleteParameters>
                                <SelectParameters>
                                    <asp:QueryStringParameter Name="role" QueryStringField="Role" Type="String" />
                                    <asp:ControlParameter ControlID="user_txt" Name="searchtag" PropertyName="Text"
                                        Type="String" DefaultValue="&quot;&quot;" />
                                    <asp:ControlParameter ControlID="searchBy_dl" Name="searchby" PropertyName="SelectedValue"
                                        Type="String" />
                                </SelectParameters>
                            </asp:ObjectDataSource>
                        </td>
                    </tr>
                </table>
            </td> 
        </tr>
    </table>
</asp:Content>