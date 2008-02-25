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
<%@ Page Language="C#" AutoEventWireup="true" EnableTheming="false" MasterPageFile="~/aspnetconfig/Util.Master" CodeBehind="ManageUser.aspx.cs" Inherits="Mainsoft.Web.AspnetConfig.ManageUser" %>
<%@ Register tagprefix="custom" namespace="Mainsoft.Web.AspnetConfig" Assembly="Mainsoft.Web.AspnetConfig"  %>
<%@ Register TagPrefix="custom" TagName="searcher" Src="~/Controls/Searcher.ascx" %>

<asp:Content ContentPlaceHolderID="SiteMapLinks" runat="server">
<td><a class="SiteMap" href="Default.aspx" title="Home page" runat="server">Home</a></td>
<td> >> </td>
<td style="color: #333333">Manage user</td>
</asp:Content>
<asp:Content runat="server" ID="main" ContentPlaceHolderID="Main">
<asp:MultiView ID="mv" ActiveViewIndex="0" runat="server">
<asp:View ID="manage" runat="server" >
    <table class="maintable">
       <tr>
            <td colspan="2" width="100%" style="height: 21px">
             Click Edit User to view or change the user's password or other properties. To assign roles to the selected user, select the appropriate check boxes on the right. To prevent users from logging in to your application while retaining their information in your database, set the status to inactive by clearing the check box. 
             <br /> <br /> <br /> 
            </td>
       </tr>
       <tr>
            <td width= "100%" valign="top" style="height: 360px">
                <table width="100%" class="innertable" >
                    <tr>
                        <td width="100%" colspan="2" class="controlheader">
                            Search for Users
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            Search by : &nbsp; &nbsp; 
                            <asp:DropDownList ID="searchBy_lbx" runat="server">
                                <asp:ListItem Selected="True" Value="Name">User Name</asp:ListItem>
                                <asp:ListItem Value="Mail">E-Mail</asp:ListItem> 
                            </asp:DropDownList> &nbsp; &nbsp;<asp:TextBox ID="user_txt" runat="server"></asp:TextBox>
                            &nbsp;
                            <asp:Button ID="searchUser_bt" runat="server" Text="Search" /> <br />
                            Wildcard characters * and ? are permitted. <br /><br />
                        </td>
                    </tr>
                    <tr>
                        <td width= "70%" valign="top" > 
                            <asp:GridView ID="Roles_gv" runat="server" CellPadding="4" DataSourceID="ObjectDataSource1" ForeColor="#333333" GridLines="None" Width="100%" AutoGenerateColumns="False" AllowPaging="True" PageSize="5" DataKeyNames="User" >
                                <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Active">
                                        <ItemTemplate>
                                            <custom:GridCheckBox ID="CheckBox1" runat="server" Checked='<%# Bind("Active") %>' AutoPostBack="true" 
                                             OnCheckedChanged="CheckBox_CheckedChanged" User='<%# Bind("User") %>' />
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Left" />
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="User" HeaderText="User Name" >
                                        <HeaderStyle HorizontalAlign="Left" />
                                    </asp:BoundField>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:HyperLink NavigateUrl='<%# string.Format("EditUser.aspx?User={0}", HttpUtility.UrlEncode((string)Eval("User"))) %>'
                                                Text="Edit User" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ShowHeader="False">
                                    <ItemTemplate>
                                          <custom:GridButton ID="GridButton1" runat="server" User='<%# Bind("User") %>' OnClick="Delete_Click" >Delete User</custom:GridButton>
                                    </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <custom:GridButton ID="grid_btn" runat="server" User='<%# Bind("User") %>' OnClick="gridbtn_click" OnLoad="gridbtn_load">Edit roles</custom:GridButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <RowStyle BackColor="#EFF3FB" />
                                <EditRowStyle BackColor="#2461BF" />
                                <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
                                <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
                                <HeaderStyle BackColor="#5D7B9D" BorderStyle="Solid" Font-Bold="True" Font-Size="0.9em"
                                    ForeColor="White" HorizontalAlign="Center" />
                                <AlternatingRowStyle BackColor="White" />
                            </asp:GridView>
                            <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="DeleteUser"
                                SelectMethod="SelectUser" TypeName="Mainsoft.Web.AspnetConfig.RolesDS">
                                <DeleteParameters>
                                    <asp:Parameter Name="User" Type="String" />
                                </DeleteParameters>
                                <SelectParameters>
                                    <asp:ControlParameter ControlID="user_txt" DefaultValue="&quot;&quot;" Name="searchtag"
                                        PropertyName="Text" Type="String" />
                                    <asp:ControlParameter ControlID="searchBy_lbx" DefaultValue="" Name="searchby" PropertyName="SelectedValue"
                                        Type="String" />
                                </SelectParameters>
                            </asp:ObjectDataSource>
                        </td>
                        <td width="30%" valign="top">
                            <custom:Searcher ID="srch" runat="server" />
                        </td>
                    </tr>
                </table>
                <br /><br />
            </td> 
        </tr>
        <tr>
            <td colspan="2">
                <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="CreateUser.aspx">Create new user</asp:HyperLink>
            </td>
        </tr>
    </table>
    </asp:View>
    <asp:View ID="quest" runat="server">
    <table class="innertable" width="60%">
        <tr>
            <td style="height: 80px" valign="top">
                <br />
                <asp:Image runat="server" ID="Img" /> &nbsp;
                Are you sure you want to delete the user: "<%= User_name %>"? <br />
                All information for this user will be deleted, including the user name, the user's membership in roles, and any profile property values associated with this user. 
                <br /><br /><br />
            </td>
        </tr>
        <tr>
            <td align="right" style="background-color:inactivecaptiontext" >
                <asp:Button ID="yes" runat="server" Text="Yes" style="width:50px" OnClick="Click_Yes" />
                <asp:Button ID="no" runat="server" Text="No" style="width:50px" OnClick="Click_No" />
            </td>
        </tr>
    </table>
    </asp:View>
    </asp:MultiView>
</asp:Content>
