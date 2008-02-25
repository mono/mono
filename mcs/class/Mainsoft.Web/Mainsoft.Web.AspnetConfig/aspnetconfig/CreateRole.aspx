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
<%@ Page Language="C#" AutoEventWireup="true" EnableTheming="false" MasterPageFile="~/aspnetconfig/Util.Master" CodeBehind="CreateRole.aspx.cs" Inherits="Mainsoft.Web.AspnetConfig.CreateRole" %>
<%@ Register TagPrefix="custom" Assembly="Mainsoft.Web.AspnetConfig" Namespace="Mainsoft.Web.AspnetConfig" %>
<asp:Content ID="Content1" ContentPlaceHolderID="SiteMapLinks" runat="server">
<td><a class="SiteMap" href="Default.aspx" title="Home page" runat="server">Home</a></td>
<td> >> </td>
<td style="color: #333333">Create\Manage roles</td>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">
    <asp:MultiView runat="server" ID="mv" ActiveViewIndex="0">
    <asp:View runat="server" id="grid">
    <table class="maintable">
        <tr>
            <td colspan="2" width="100%" style="height: 21px">
                You can optionally add roles, or groups, that enable you to allow or deny groups of users access to specific folders in your Web site. For example, you might create roles such as "managers," "sales," or "members," each with different access to specific folders.  <br /> <br />
            </td>
        </tr>
        <tr>
            <td width= "100%" valign="top">
                <table width="100%" class="innertable" >
                    <tr>
                        <td width="100%" class="controlheader">
                            Create New Role
                        </td>
                    </tr>
                    <tr>
                        <td>
                            New role name:  &nbsp; &nbsp; 
                            <asp:TextBox ID="roleName_txb" runat="server" /> &nbsp; &nbsp;
                            <asp:Button  ID="roleName_bt"  runat="server" Text="Add role" OnClick="roleName_bt_Click" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="error_lb" runat="server" ForeColor="Red" />
                            <br /> <br />
                        </td>
                    </tr>
                </table>
            </td> 
        </tr>
        <tr>
            <td width= "100%" valign="top" datakeynames="Role;">
                <asp:GridView ID="Roles_gv" runat="server" CellPadding="4" DataSourceID="ObjectDataSource1" ForeColor="#333333" GridLines="None" Width="100%" AutoGenerateColumns="False" DataKeyNames="Role">
                    <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                    <Columns>
                        <asp:BoundField DataField="Role" HeaderText="Role Name" >
                            <HeaderStyle HorizontalAlign="Left" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Add\Remove Role">
                            <ItemTemplate>
                                <asp:HyperLink ID="HyperLink1" NavigateUrl='<%# string.Format("ManageRole.aspx?Role={0}", HttpUtility.UrlEncode((string)Eval("Role"))) %>'
                                    Text="Manage" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                  <custom:GridButton ID="grid_btn" runat="server" User='<%# Bind("Role") %>' OnClick="gridbtn_click"  >Delete</custom:GridButton>
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
                <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="Delete"
                    SelectMethod="Select" TypeName="Mainsoft.Web.AspnetConfig.RolesDS">
                    <DeleteParameters>
                        <asp:Parameter Name="Role" Type="String" />
                    </DeleteParameters>
                </asp:ObjectDataSource>
            </td>
        </tr>
    </table>
    </asp:View>
    <asp:View runat="server" ID="yesno" >
    <table class="innertable" width="60%">
        <tr>
            <td style="height: 80px" valign="top">
                <br />
                    <asp:Image runat="server" id="Img" /> &nbsp;
                    Are you sure you want to delete the role: "<%= Role %>"?
                <br />
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
   