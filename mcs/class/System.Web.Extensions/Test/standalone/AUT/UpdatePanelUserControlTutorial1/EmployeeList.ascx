<%@ Control Language="C#" AutoEventWireup="true" Inherits="UpdatePanelUserControl.EmployeeList" Codebehind="EmployeeList.ascx.cs" %>
<asp:UpdatePanel ID="EmployeeListUpdatePanel" runat="server">
  <ContentTemplate>
    <asp:Label ID="LastUpdatedLabel" runat="server"></asp:Label>
    <asp:GridView ID="EmployeesGridView" runat="server" AllowPaging="True" AllowSorting="True"
        AutoGenerateColumns="False" CellPadding="4" DataKeyNames="EmployeeID" DataSourceID="EmployeesDataSource"
        ForeColor="#333333" GridLines="None">
      <FooterStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
      <Columns>
        <asp:CommandField ShowSelectButton="True">
          <ItemStyle Width="50px" />
        </asp:CommandField>
        <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName">
          <ItemStyle Width="120px" />
          <HeaderStyle HorizontalAlign="Left" />
        </asp:BoundField>
        <asp:BoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName">
          <ItemStyle Width="100px" />
          <HeaderStyle HorizontalAlign="Left" />
        </asp:BoundField>
      </Columns>
      <RowStyle BackColor="#FFFBD6" ForeColor="#333333" />
      <SelectedRowStyle BackColor="#FFCC66" Font-Bold="True" ForeColor="Navy" />
      <PagerStyle BackColor="#FFCC66" ForeColor="#333333" HorizontalAlign="Center" />
      <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
      <AlternatingRowStyle BackColor="White" />
    </asp:GridView>
    <asp:SqlDataSource ID="EmployeesDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:AdventureWorksConnectionString %>"
        SelectCommand="SELECT HumanResources.Employee.EmployeeID, Person.Contact.LastName, Person.Contact.FirstName FROM Person.Contact INNER JOIN HumanResources.Employee ON Person.Contact.ContactID = HumanResources.Employee.ContactID ORDER BY Person.Contact.LastName, Person.Contact.FirstName">
    </asp:SqlDataSource>
  </ContentTemplate>
</asp:UpdatePanel>
