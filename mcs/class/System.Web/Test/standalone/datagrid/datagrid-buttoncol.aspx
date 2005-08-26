<!-- based on bug 49738, 50171 -->
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
		<p>Should have the company names with alternating
		black on orange / white on red colors. The push buttons are not
		colored. The last column should use a format string Symbol: {0}</p>
		<form runat="server">
			<asp:DataGrid id="DataGrid1" AutoGenerateColumns=False runat="server">
				<ItemStyle BackColor=orange ForeColor=black />
				<AlternatingItemStyle BackColor=red ForeColor=white />
				<Columns>
					<asp:BoundColumn DataField="Company" />
					<asp:ButtonColumn Text="Test" />
					<asp:buttoncolumn ButtonType="PushButton" Text="push" />
					<asp:ButtonColumn ButtonType=LinkButton DataTextField="Symbol" DataTextFormatString="Symbol: {0}" />
				</Columns>									
			</asp:DataGrid>
			
		</form>
	</body>
</html>
