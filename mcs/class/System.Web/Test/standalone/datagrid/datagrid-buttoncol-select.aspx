<!-- based on bug 49744 -->
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
		<p>Clicking select should highlight a column</p>
		<form runat="server">
			<asp:DataGrid id="DataGrid1" GridLines=Horizontal AutoGenerateColumns=False runat="server">
				<ItemStyle BackColor=orange ForeColor=black />
				<AlternatingItemStyle BackColor=red ForeColor=white />
				<SelectedItemStyle BackColor=#33ffff ForeColor=black />
				<Columns>
					<asp:BoundColumn DataField="Company" />
					<asp:ButtonColumn Text="Select" CommandName="Select" />
				</Columns>									
			</asp:DataGrid>
			
		</form>
	</body>
</html>
