<!-- <Snippet2> -->
<%@ Control Language="C#" AutoEventWireup="true" Inherits="UpdatePanelUserControl.EmployeeInfo" Codebehind="EmployeeInfo.ascx.cs" %>
<asp:UpdatePanel ID="EmployeeInfoUpdatePanel" runat="server">
  <ContentTemplate>
    <asp:Label ID="LastUpdatedLabel" runat="server"></asp:Label>
    <asp:DetailsView ID="EmployeeDetailsView" runat="server" Height="50px" Width="410px" AutoGenerateRows="False" BackColor="LightGoldenrodYellow" BorderColor="Tan" BorderWidth="1px" CellPadding="2" DataSourceID="EmployeeDataSource" ForeColor="Black" GridLines="None">
      <FooterStyle BackColor="Tan" />
      <EditRowStyle BackColor="DarkSlateBlue" ForeColor="GhostWhite" />
      <PagerStyle BackColor="PaleGoldenrod" ForeColor="DarkSlateBlue" HorizontalAlign="Center" />
      <Fields>
        <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
        <asp:BoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName" />
        <asp:BoundField DataField="EmailAddress" HeaderText="E-mail Address" SortExpression="EmailAddress" />
        <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
        <asp:BoundField DataField="HireDate" HeaderText="Hire Date" SortExpression="HireDate" />
        <asp:BoundField DataField="VacationHours" HeaderText="Vacation Hours" SortExpression="VacationHours" />
        <asp:BoundField DataField="SickLeaveHours" HeaderText="Sick Leave Hours" SortExpression="SickLeaveHours" />
      </Fields>
      <HeaderStyle BackColor="Tan" Font-Bold="True" />
      <AlternatingRowStyle BackColor="PaleGoldenrod" />
    </asp:DetailsView>
    <asp:SqlDataSource ID="EmployeeDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:AdventureWorksConnectionString %>"
        SelectCommand="SELECT Person.Contact.LastName, Person.Contact.FirstName, Person.Contact.EmailAddress, Person.Contact.Phone, HumanResources.Employee.HireDate, HumanResources.Employee.VacationHours, HumanResources.Employee.SickLeaveHours FROM Person.Contact INNER JOIN HumanResources.Employee ON Person.Contact.ContactID = HumanResources.Employee.ContactID WHERE HumanResources.Employee.EmployeeID = @SelectedEmployeeID">
      <SelectParameters>
        <asp:Parameter Name="SelectedEmployeeID" />
      </SelectParameters>
 </asp:SqlDataSource>
</ContentTemplate>
</asp:UpdatePanel>
<!-- </Snippet2> -->
