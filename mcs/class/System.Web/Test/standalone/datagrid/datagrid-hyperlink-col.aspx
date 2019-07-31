<!-- Based on bug 50234, 51092 -->
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
		</script>
	</head>
	<body>
		<form runat="server">
			<asp:DataGrid id="DataGrid1" AutoGenerateColumns=False runat="server">
			<Columns>
				<asp:HyperLinkColumn Text="" DataNavigateUrlField="Company" />
				<asp:HyperLinkColumn Text="ClickHere" DataNavigateUrlField="Company" />
				<asp:HyperLinkColumn Text="Get quote" DataNavigateUrlField="Symbol" DataNavigateUrlFormatString="~/quote.aspx?{0}" />
				<asp:HyperLinkColumn Text="Example quote" DataNavigateUrlField="Symbol" DataNavigateUrlFormatString="http://www.example.com/search?q=stocks:{0}" />
			</Columns>
			</asp:DataGrid>
		</form>
	</body>
</html>
