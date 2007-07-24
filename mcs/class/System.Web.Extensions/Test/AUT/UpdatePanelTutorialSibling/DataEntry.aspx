<%@ Page Language="C#" %>
<%@ Import Namespace="System.Collections.Generic" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Enter New Employees</title>
    <script runat="server">
        private List<Employee> EmployeeList;

        protected void Page_Load()
        {
            if (!IsPostBack)
            {
                EmployeeList = new List<Employee>();
                EmployeeList.Add(new Employee(1, "Jump", "Dan"));
                EmployeeList.Add(new Employee(2, "Kirwan", "Yvette"));
                ViewState["EmployeeList"] = EmployeeList;
            }
            else
                EmployeeList = (List<Employee>)ViewState["EmployeeList"];
            
            EmployeesGridView.DataSource = EmployeeList;
            EmployeesGridView.DataBind();
        }
        
        protected void InsertButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(FirstNameTextBox.Text) ||
               String.IsNullOrEmpty(LastNameTextBox.Text)) { return; }

            int employeeID = EmployeeList[EmployeeList.Count-1].EmployeeID + 1;
            
            string lastName = Server.HtmlEncode(FirstNameTextBox.Text);
            string firstName = Server.HtmlEncode(LastNameTextBox.Text);
            
            FirstNameTextBox.Text = String.Empty;
            LastNameTextBox.Text = String.Empty;
            
            EmployeeList.Add(new Employee(employeeID, lastName, firstName));
            ViewState["EmployeeList"] = EmployeeList;
            
            EmployeesGridView.DataBind();
            EmployeesGridView.PageIndex = EmployeesGridView.PageCount;
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            FirstNameTextBox.Text = String.Empty;
            LastNameTextBox.Text = String.Empty;
        }
    
        [Serializable]
        public class Employee
        {
            private int _employeeID;
            private string _lastName;
            private string _firstName;

            public int EmployeeID
            {
                get { return _employeeID; }
            }
            
            public string LastName
            {
                get { return _lastName; }
            }
            
            public string FirstName
            {
                get { return _firstName; }
            }
            
            public Employee(int employeeID, string lastName, string firstName)
            {
                _employeeID = employeeID;
                _lastName = lastName;
                _firstName = firstName;
            }
        }

    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        &nbsp;</div>
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />
        <table>
            <tr>
                <td style="height: 206px" valign="top">
                    <asp:UpdatePanel ID="InsertEmployeeUpdatePanel" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                          <table cellpadding="2" border="0" style="background-color:#7C6F57">
                            <tr>
                              <td><asp:Label ID="FirstNameLabel" runat="server" AssociatedControlID="FirstNameTextBox" 
                                             Text="First Name" ForeColor="White" /></td>
                              <td><asp:TextBox runat="server" ID="FirstNameTextBox" /></td>
                            </tr>
                            <tr>
                              <td><asp:Label ID="LastNameLabel" runat="server" AssociatedControlID="LastNameTextBox" 
                                             Text="Last Name" ForeColor="White" /></td>
                              <td><asp:TextBox runat="server" ID="LastNameTextBox" /></td>
                            </tr>
                            <tr>
                              <td></td>
                              <td>
                                <asp:LinkButton ID="InsertButton" runat="server" Text="Insert" OnClick="InsertButton_Click" ForeColor="White" />
                                <asp:LinkButton ID="Cancelbutton" runat="server" Text="Cancel" OnClick="CancelButton_Click" ForeColor="White" />
                              </td>
                            </tr>
                          </table>
                          <asp:Label runat="server" ID="InputTimeLabel"><%=DateTime.Now %></asp:Label>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
                <td style="height: 206px" valign="top">
                    <asp:UpdatePanel ID="EmployeesUpdatePanel" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:GridView ID="EmployeesGridView" runat="server" BackColor="LightGoldenrodYellow" BorderColor="Tan"
                                BorderWidth="1px" CellPadding="2" ForeColor="Black" GridLines="None" AutoGenerateColumns="False">
                                <FooterStyle BackColor="Tan" />
                                <SelectedRowStyle BackColor="DarkSlateBlue" ForeColor="GhostWhite" />
                                <PagerStyle BackColor="PaleGoldenrod" ForeColor="DarkSlateBlue" HorizontalAlign="Center" />
                                <HeaderStyle BackColor="Tan" Font-Bold="True" />
                                <AlternatingRowStyle BackColor="PaleGoldenrod" />
                                <Columns>
                                    <asp:BoundField DataField="EmployeeID" HeaderText="Employee ID" />
                                    <asp:BoundField DataField="LastName" HeaderText="Last Name" />
                                    <asp:BoundField DataField="FirstName" HeaderText="First Name" />
                                </Columns>
                                <PagerSettings PageButtonCount="5" />
                            </asp:GridView>
                            <asp:Label runat="server" ID="ListTimeLabel"><%=DateTime.Now %></asp:Label>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="InsertButton" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
