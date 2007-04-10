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
<%@ Page Language="C#" AutoEventWireup="true" EnableTheming="false" MasterPageFile="~/aspnetconfig/Util.Master" CodeBehind="EditUser.aspx.cs" Inherits="Mainsoft.Web.AspnetConfig.EditUser" %>
<%@ Register TagPrefix="custom" TagName="searcher" Src="~/Controls/Searcher.ascx" %>
<asp:Content ContentPlaceHolderID="SiteMapLinks" runat="server">
<td><a class="SiteMap" href="Default.aspx" title="Home page" runat="server">Home</a></td>
<td> >> </td>
<td style="color: #284E98"><a class="SiteMap" href="ManageUser.aspx" title="Manage user" runat="server">Manage user</a></td>
<td> >> </td>
<td style="color: #333333">Edit user</td>
</asp:Content>
<asp:Content runat="server" ID="main" ContentPlaceHolderID="Main">
    <asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
        <asp:View ID="updateUser" runat="server" >
        <div>
        <table width="100%" class="maintable">
            <tr>
                <td colspan="2" width="100%" style="height: 21px">
                    Use this page to edit user information and to specify what roles a user belongs in. 
                     <br /> <br /> <br /> 
                </td>
            </tr>
            <tr>
                <td width="70%" class="innertable" align="left" valign="top">
                    <table width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                            <td width="100%" class="controlheader" colspan="3">
                                User
                            </td>
                        </tr>
                        <tr>
                            <td>
                                User ID:
                            </td>
                            <td>
                                <asp:TextBox ID="userid_txb" runat="server" Enabled="false" />
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td>
                               <span style="font-size: 20px; color: red">*</span> E-mail address: 
                            </td>
                            <td>
                                <asp:TextBox ID="email_txb" runat="server"></asp:TextBox>
                            </td>
                            <td>
                                <asp:CheckBox ID="active_chb" runat="server" /> Active user
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Description:
                            </td>
                            <td>
                                <asp:TextBox ID="desc_txb" runat="server">[not set]</asp:TextBox>
                            </td>
                            <td>
                                <asp:Button ID="save_bt" runat="server" Text="Save" OnClick="save_bt_Click" />
                            </td>
                        </tr>
                    </table>
                </td>
                <td class="innertable" valign="top" style="width: 82%" >
                   <custom:searcher ID="srch" runat="server" />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    (<span style="font-size: 20px; color: red">*</span>)Required field <br /><br />
                    <asp:RequiredFieldValidator ID="required_email_validator" runat="server" ControlToValidate="email_txb" ErrorMessage="Email required" />
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="email_txb" ValidationExpression="\S+@\S+\.\S+" runat="server" ErrorMessage="Email format is invalid"></asp:RegularExpressionValidator>
                </td>
            </tr>
        </table>
        </div>
        </asp:View>
        <asp:View ID="successupdate" runat="server">
            <div>
            <table width="100%" class="maintable">
            <tr>
                <td width="100%" class="controlheader">
                    User Management
                </td>
            </tr>
            <tr>
                <td width="100%" style="height: 100px" valign="top" align="left">
                    You have successfully updated the user
                    <asp:Label ID="name_lbl" runat="server" /> <br /> <br /> <br />
                </td>
            </tr>
            <tr>
                <td width="100%" align="right">
                    <asp:Button ID="success_btn" runat="server" Text="OK" OnClick="success_btn_Click" />
                </td>
            </tr>
            </table>
            </div>
        </asp:View>
    </asp:MultiView>
</asp:Content>
