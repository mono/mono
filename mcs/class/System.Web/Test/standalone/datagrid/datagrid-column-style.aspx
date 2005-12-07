<%@ Page language="c#" %>
<%@ Import namespace="System.Data" %>
<html>
<head>
<script runat="server">
        void Page_Load(object sender, System.EventArgs e)
	{
		DataTable dt = new DataTable();
		dt.Columns.Add("col1");
		dt.Columns.Add("col2");
		DataRow dr = dt.NewRow();
		dt.Rows.Add(dr);
		dt.Rows[0]["col1"] = "Center";
		dt.Rows[0]["col2"] = "Left";
		DataGrid1.DataSource = dt.DefaultView;
		DataGrid1.DataBind();
	}
</script>
<title>DataGridTest</title>
</head>
<body>
The 'Center' word should be centered.
<br>
<form id="Form1" method="post" runat="server">
	<asp:DataGrid id="DataGrid1" runat="server" AutoGenerateColumns="False">
	<Columns>
		<asp:BoundColumn ItemStyle-HorizontalAlign="Center" HeaderText="Column1 23456789" DataField="col1"></asp:BoundColumn>
		<asp:BoundColumn ItemStyle-HorizontalAlign="Left" HeaderText="Column2 34567891" DataField="col2"></asp:BoundColumn>
	</Columns>
	</asp:DataGrid>
</form>
</body>
</html>
