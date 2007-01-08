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
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/aspnetconfig/Util.Master" CodeBehind="CreateUser.aspx.cs" Inherits="Mainsoft.Web.AspnetConfig.CreateUser" %>
<asp:Content runat="server" ID="main" ContentPlaceHolderID="Main">
    <table style="border-right: thin solid; padding-right: 2px; border-top: thin solid; padding-left: 2px; padding-bottom: 2px; border-left: thin solid; padding-top: 2px; border-bottom: thin solid">
        <tr>
            <td colspan="2" width="100%" style="height: 21px">
                <asp:Label ID="text_lbl" runat="server" Text="Add a user by entering a user name, password, and e-mail address on this page." />
            </td>
        </tr>
        <tr>
            <td width= "60%" valign="top">
                    <asp:CreateUserWizard ID="CreateUserWizard1" runat="server" BackColor="#F7F6F3"
                    BorderColor="#E6E2D8" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" LoginCreatedUser="false"
                    Font-Size="0.8em" Width="353px" ContinueDestinationPageUrl="CreateUser.aspx" FinishDestinationPageUrl="CreateUser.aspx" EmailRegularExpression="\S+@\S+\.\S+" >
                    <WizardSteps>
                        <asp:CreateUserWizardStep ID="CreateUserWizardStep1" runat="server">
                        </asp:CreateUserWizardStep>
                        <asp:CompleteWizardStep ID="CompleteWizardStep1" runat="server">
                        </asp:CompleteWizardStep>
                    </WizardSteps>
                    <SideBarStyle BackColor="#5D7B9D" BorderWidth="0px" Font-Size="0.9em" VerticalAlign="Top" />
                    <TitleTextStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <SideBarButtonStyle BorderWidth="0px" Font-Names="Verdana" ForeColor="White" />
                    <NavigationButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" BorderStyle="Solid"
                        BorderWidth="1px" Font-Names="Verdana" ForeColor="#284775" />
                    <HeaderStyle BackColor="#5D7B9D" BorderStyle="Solid" Font-Bold="True" Font-Size="0.9em"
                        ForeColor="White" HorizontalAlign="Center" />
                    <CreateUserButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" BorderStyle="Solid"
                        BorderWidth="1px" Font-Names="Verdana" ForeColor="#284775" />
                    <ContinueButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" BorderStyle="Solid"
                        BorderWidth="1px" Font-Names="Verdana" ForeColor="#284775" />
                    <StepStyle BorderWidth="0px" />
                </asp:CreateUserWizard>
            </td>
            <td width= "60%" valign="top">
                <table width="100%" style="background-color: #f7f6f3; border-right: #e6e2d8 1px solid; border-top: #e6e2d8 1px solid; border-left: #e6e2d8 1px solid; border-bottom: #e6e2d8 1px solid;" >
                    <tr>
                        <td width="100%" style="text-align: center; background-color: #5D7B9D; font-weight:bold; font-size:0.9em; color:White;text-align:center; height: 19px;">
                           Roles
                        </td>
                    </tr>
                    <tr>
                        <td valign="top">
                            &nbsp;<asp:CheckBoxList ID="roles_lst" runat="server">
                            </asp:CheckBoxList></td>
                    </tr>
                </table>
            </td> 
        </tr>
        <tr>
            <td colspan="2">
                Active User
                <asp:CheckBox ID="active_chb" runat="server" Checked="true" Enabled="true" />                
            </td>
        </tr>
    </table>
</asp:Content>
