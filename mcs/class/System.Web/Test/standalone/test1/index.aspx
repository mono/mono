<%@ Page language="c#" src="./index.aspx.cs"  Inherits="test.SimplePage" AutoEventWireup="false"%>

<html>
<head>
</head>
<body>

<asp:DataGrid id="testGrid" runat="server" AutoGenerateColumns="true" CellPadding="5"
  HeaderStyle-BackColor="PapayaWhip" BorderWidth="2px" BorderColor="#000099"
  AlternatingItemStyle-BackColor="LightGray" HeaderStyle-Font-Bold
  EditItemStyle-BackColor="Yellow" EditItemStyle-ForeColor="Black" DataKeyField="id">
</asp:DataGrid>

</body>
</html>

