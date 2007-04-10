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
<%@ Page Language="C#" AutoEventWireup="true" EnableTheming="false" MasterPageFile="~/aspnetconfig/Util.Master" CodeBehind="CreateUser.aspx.cs" Inherits="Mainsoft.Web.AspnetConfig.CreateUser" %>
<asp:Content ContentPlaceHolderID="SiteMapLinks" runat="server">
<td><a class="SiteMap" href="Default.aspx" title="Home page" runat="server">Home</a></td>
<td> >> </td>
<td style="color: #333333">Create user</td>
</asp:Content>
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
                    Font-Size="0.8em" Width="353px" ContinueDestinationPageUrl="CreateUser.aspx" FinishDestinationPageUrl="CreateUser.aspx" EmailRegularExpression="\S+@\S+\.\S+">
                    <WizardSteps>
                        <asp:CreateUserWizardStep ID="CreateUserWizardStep1" runat="server">
                            <ContentTemplate>
                                <table border="0" style="font-size: 100%; width: 353px; font-family: Verdana">
                                    <tr>
                                        <td align="center" colspan="2" style="font-weight: bold; color: white; background-color: #5d7b9d;
                                            text-align: left">
                                            Sign Up for Your New Account</td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">User Name:</asp:Label></td>
                                        <td>
                                            <asp:TextBox ID="UserName" runat="server"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                ErrorMessage="User Name is required." ToolTip="User Name is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password:</asp:Label></td>
                                        <td>
                                            <asp:TextBox ID="Password" runat="server" TextMode="Password"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                ErrorMessage="Password is required." ToolTip="Password is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <asp:Label ID="ConfirmPasswordLabel" runat="server" AssociatedControlID="ConfirmPassword">Confirm Password:</asp:Label></td>
                                        <td>
                                            <asp:TextBox ID="ConfirmPassword" runat="server" TextMode="Password"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="ConfirmPasswordRequired" runat="server" ControlToValidate="ConfirmPassword"
                                                ErrorMessage="Confirm Password is required." ToolTip="Confirm Password is required."
                                                ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <asp:Label ID="EmailLabel" runat="server" AssociatedControlID="Email">E-mail:</asp:Label></td>
                                        <td>
                                            <asp:TextBox ID="Email" runat="server"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                                                ErrorMessage="E-mail is required." ToolTip="E-mail is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <asp:Label ID="QuestionLabel" runat="server" AssociatedControlID="Question">Security Question:</asp:Label></td>
                                        <td>
                                            <asp:TextBox ID="Question" runat="server"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="QuestionRequired" runat="server" ControlToValidate="Question"
                                                ErrorMessage="Security question is required." ToolTip="Security question is required."
                                                ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <asp:Label ID="AnswerLabel" runat="server" AssociatedControlID="Answer">Security Answer:</asp:Label></td>
                                        <td>
                                            <asp:TextBox ID="Answer" runat="server"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="AnswerRequired" runat="server" ControlToValidate="Answer"
                                                ErrorMessage="Security answer is required." ToolTip="Security answer is required."
                                                ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="center" colspan="2">
                                            <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password"
                                                ControlToValidate="ConfirmPassword" Display="Dynamic" ErrorMessage="The Password and Confirmation Password must match."
                                                ValidationGroup="CreateUserWizard1"></asp:CompareValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="center" colspan="2">
                                            <asp:RegularExpressionValidator ID="EmailRegExp" runat="server" ControlToValidate="Email"
                                                Display="Dynamic" ErrorMessage="Please enter a different e-mail." ValidationExpression="\S+@\S+\.\S+"
                                                ValidationGroup="CreateUserWizard1"></asp:RegularExpressionValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="center" colspan="2" style="color: red">
                                            <asp:Literal ID="ErrorMessage" runat="server" EnableViewState="False"></asp:Literal>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </asp:CreateUserWizardStep>
                        <asp:CompleteWizardStep ID="CompleteWizardStep1" runat="server">
                            <ContentTemplate>
                                <table border="0" style="font-size: 100%; width: 353px; font-family: Verdana">
                                    <tr>
                                        <td align="center" colspan="2" style="font-weight: bold; color: white; background-color: #5d7b9d;
                                            text-align: left">
                                            Create User</td>
                                    </tr>
                                    <tr>
                                        <td>
                                            &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;
                                            <br />
                                            &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                                            &nbsp;&nbsp; Complete.<br />
                                            &nbsp; &nbsp; &nbsp;Your account has been successfully created.</td>
                                    </tr>
                                    <tr>
                                        <td align="right" colspan="2">
                                            <asp:Button ID="ContinueButton" runat="server" BackColor="#FFFBFF" BorderColor="#CCCCCC"
                                                BorderStyle="Solid" BorderWidth="1px" CausesValidation="False" CommandName="Continue"
                                                Font-Names="Verdana" ForeColor="#284775" Text="Continue" ValidationGroup="CreateUserWizard1" />
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
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
