<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void CustomValidator1_ServerValidate(object source, ServerValidateEventArgs args)
    {
        // Business logic to determine if performance is available.
        // For demonstration we are not allowing matinee tickets.
        args.IsValid = false;
        if (DropDownList1.SelectedIndex > 0)
        {
            args.IsValid = true;
        }
    }

    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        TextBox3.Text = Calendar1.SelectedDate.ToShortDateString();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        CompareValidator1.ValueToCompare = DateTime.Today.ToShortDateString();
    }

</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Wizard Inside UpdatePanel</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
            <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <asp:Wizard ID="Wizard1" runat="server" ActiveStepIndex="0" BackColor="#EFF3FB"
                    BorderColor="#B5C7DE" BorderWidth="1px" Font-Names="Verdana"
                    Font-Size="0.8em" Width="460px">
                    <StepStyle Font-Size="0.8em" ForeColor="#333333" />
                    <SideBarStyle BackColor="#507CD1" Font-Size="0.9em" VerticalAlign="Top" />
                    <NavigationButtonStyle BackColor="White" BorderColor="#507CD1"
                        BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana"
                        Font-Size="0.8em" ForeColor="#284E98" />
                    <WizardSteps>
                        <asp:WizardStep ID="WizardStep1" runat="server" StepType="Start" Title="Specify Tickets">
                            <asp:Label ID="Label1" runat="server" Text="Enter a number 1 to 10:"></asp:Label>
                            <asp:TextBox ID="TextBox1" runat="server" Width="50px"></asp:TextBox>
                            <br />
                            <asp:RangeValidator ID="RangeValidator1" runat="server" ControlToValidate="TextBox1"
                                Display="None" ErrorMessage="Incorrect number of tickets." MaximumValue="10"
                                MinimumValue="1" Type="Integer"></asp:RangeValidator>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server"
                                ControlToValidate="TextBox1" Display="None" ErrorMessage="Enter a number of tickets." ></asp:RequiredFieldValidator><br />
                            <asp:Label ID="Label2" runat="server" Text="Enter an email address:"></asp:Label>
                            <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
                            <br />
                            <asp:RegularExpressionValidator ID="RegularExpressionValidator1"
                                runat="server" ControlToValidate="TextBox2" Display="None"
                                ErrorMessage="The email was not correctly formatted." 
                                ValidationExpression="^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$"
                                ></asp:RegularExpressionValidator>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server"
                                ControlToValidate="TextBox2" Display="None" ErrorMessage="Email is required."
                                ></asp:RequiredFieldValidator><br />
                            <asp:ValidationSummary ID="Step1ValidationSummary" 
                            runat="server" />
                        </asp:WizardStep>
                        <asp:WizardStep ID="WizardStep2" runat="server" Title="Select A Date" StepType="Finish">
                            <div style="text-align: center">
                                <asp:Calendar ID="Calendar1" runat="server" OnSelectionChanged="Calendar1_SelectionChanged">
                                </asp:Calendar>
                            </div>
                            <br />
                            <asp:Label ID="Label3" runat="server" Text="Choose a date to attend:"></asp:Label>
                            <asp:TextBox ID="TextBox3" runat="server"></asp:TextBox>
                            <br />
                            <asp:CompareValidator ID="CompareValidator1" runat="server" ControlToValidate="TextBox3"
                                ErrorMessage="Pick a date in the future." Operator="GreaterThanEqual" 
                                Type="Date" Display="None"></asp:CompareValidator>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" 
                                ControlToValidate="TextBox3" Display="None" ErrorMessage="Date is required."></asp:RequiredFieldValidator><br />
                            Choose a performance:
                            <asp:DropDownList ID="DropDownList1" runat="server">
                                <asp:ListItem>matinee</asp:ListItem>
                                <asp:ListItem>early evening</asp:ListItem>
                                <asp:ListItem>late evening</asp:ListItem>
                            </asp:DropDownList>
                            <br />
                            <asp:CustomValidator ID="CustomValidator1" Display="None" runat="server" ControlToValidate="DropDownList1"
                                ErrorMessage="That performance is not available." OnServerValidate="CustomValidator1_ServerValidate"></asp:CustomValidator><br />
                            <asp:ValidationSummary ID="ValidationSummary2" 
                            runat="server" />
                        </asp:WizardStep>
                        <asp:WizardStep ID="WizardStep3" runat="server" StepType="Complete" Title="See You There!">
                            Your tickets will be sent in email.
                        </asp:WizardStep>
                    </WizardSteps>
                    <SideBarButtonStyle BackColor="#507CD1" Font-Names="Verdana"
                        ForeColor="White" />
                    <HeaderStyle BackColor="#284E98" BorderColor="#EFF3FB" BorderStyle="Solid"
                        BorderWidth="2px" Font-Bold="True" Font-Size="0.9em" ForeColor="White"
                        HorizontalAlign="Center" />
                    <HeaderTemplate>
                        Order Your Complimentary Tickets
                    </HeaderTemplate>
                </asp:Wizard>            
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>

