<!-- based on bug 49736 -->
<%@ Import Namespace="System.Data" %>
<%@ Page language="c#" %>
<html>
	<head>
		<script runat="server">
		void Page_Load (object sender, EventArgs e)
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
		<p>Should have the company names with an "edit" button next to each. The button won't work</p>
		<form runat="server">
			<asp:DataGrid id="DataGrid1" AutoGenerateColumns=False runat="server">
				<Columns>
					<asp:BoundColumn DataField="Company" />
					<asp:EditCommandColumn CancelText="Cancel" EditText="Edit" UpdateText="Update" />
				</Columns>									
			</asp:DataGrid>
			
		</form>
	</body>
</html>
