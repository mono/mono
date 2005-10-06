<!-- Based on bug 50173 -->
<%@ Page language="c#" %>
<%@ Import Namespace="System.Data" %>
<html>
	<head>
		<script runat="server">
		private void Page_Load(object sender, System.EventArgs e)
		{
			if (IsPostBack)
				return;
			DataSet ds = new DataSet ();
			ds.ReadXml (new System.IO.StringReader (@"
<DataSet>
	<Stocks Company='Novell Inc.'     Symbol='NOVL' Price='6.14'   />
	<Stocks Company='Microsoft Corp.' Symbol='MSFT' Price='25.92'  />
	<Stocks Company='Google'          Symbol='GOOG' Price='291.60' />
</DataSet>
"));
			DataGrid1.DataSource = ds;
			DataGrid1.DataBind();
		}
		
		private void DataGrid1_ItemCreated(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
		{
			if(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) 
				e.Item.Cells[0].Controls.Add( new LiteralControl ("Hello World" ));
		}
		</script>
	</head>
	<body>
		<form runat="server">
			<asp:DataGrid id="DataGrid1" AutoGenerateColumns=False OnItemCreated="DataGrid1_ItemCreated" runat="server">
			<Columns>
				<asp:ButtonColumn ButtonType=LinkButton ItemStyle-ForeColor=Red DataTextField="Company" DataTextFormatString="Blah: {0}" />
			</Columns>
			</asp:DataGrid>
		</form>
	</body>
</html>
