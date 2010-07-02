<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div><%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><asp:GridView id="GridView1" runat="server" AutoGenerateColumns="false" AllowSorting="true" DataSourceID="dataSource1"
	SortedAscendingCellStyle-BackColor="LightYellow" SortedAscendingHeaderStyle-BackColor="Yellow"
	SortedDescendingCellStyle-BackColor="AliceBlue" SortedDescendingHeaderStyle-BackColor="LightBlue">
	<Columns>
		<asp:BoundField SortExpression="ProductName" DataField="ProductName" HeaderText="Name" />
		<asp:BoundField SortExpression="ProductID" DataField="ProductID" HeaderText="ID" />
	</Columns>
    </asp:GridView><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
    <asp:ObjectDataSource ID="dataSource1" runat="server" TypeName="TestDatabase" SelectMethod="GetData"
			  DataObjectTypeName="TestData" SortParameterName="sortExpression" />
    </div>
    </form>
</body>
</html>
