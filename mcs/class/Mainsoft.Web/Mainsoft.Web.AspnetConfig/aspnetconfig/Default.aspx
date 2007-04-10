<%--
// Mainsoft.Web.AspnetConfig - Site AspnetConfig utility
// Authors:
//  Klain Yoni <yonik@mainsoft.com>
//
// Mainsoft.Web.AspnetConfig - Site AspnetConfig utility
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
<%@ Page Language="C#" AutoEventWireup="true" EnableTheming="false" MasterPageFile="~/aspnetconfig/Util.Master" CodeBehind="Default.aspx.cs" Inherits="Mainsoft.Web.AspnetConfig.Default" %>
<asp:Content ContentPlaceHolderID="SiteMapLinks" runat="server">
<td style="color: #333333">Home</td>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Main">
    <table>
        <tr>
            <td>
                You can use the Web Site Administration Tool to manage all the security settings for your application. You can set up users and passwords (authentication), and create roles (groups of users). By default, user information is stored in a Cloudscape database in the Data folder of your Web project. If you want to store user information in a different database, you may configure a different provider in the web.config file.
                <br /><br /><br />
            </td>
        </tr>
        <tr>
            <td>
                <table class="innertable" width="70%" >
                    <tr>
                        <td class="controlheader">
                            Users
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <%= User_count %>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="CreateUser.aspx" OnLoad="UsersLinks_Load">Create user</asp:HyperLink>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:HyperLink ID="HyperLink2" runat="server" NavigateUrl="ManageUser.aspx"  OnLoad="UsersLinks_Load">Manage users</asp:HyperLink>
                        </td>
                    </tr>
                </table> 
                <br /><br /><br />
            </td>
        </tr>
        <tr>
            <td>
                <table class="innertable" width="70%">
                    <tr>
                        <td class="controlheader">
                            Roles 
                        </td>
                    </tr>
                    <tr>
                        <td>
                             <%= Roles_count %>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:HyperLink ID="HyperLink3" runat="server" NavigateUrl="CreateRole.aspx" OnLoad="HyperLink1_Load" >Create or manage roles</asp:HyperLink>
                        </td>
                    </tr>
                </table>
                <br /><br /><br />
            </td>
        </tr>
    </table>
</asp:Content>