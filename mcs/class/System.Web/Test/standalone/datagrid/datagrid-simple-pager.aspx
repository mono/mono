<!-- based on bug# 49837

There may be a regression here in Microsoft's v2.0. See: 
http://lab.msdn.microsoft.com/ProductFeedback/viewFeedback.aspx?feedbackId=FDBK33880
-->
<%@ Import Namespace="System.Data" %>
<%@ Page language="c#" %>
<html>
	<head>
		<script runat="server">
		void Page_Load (object sender, EventArgs e)
		{
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

		void changepage(object source, DataGridPageChangedEventArgs e)
		{
			DataGrid1.CurrentPageIndex = e.NewPageIndex;
			DataGrid1.DataBind ();
		}
		</script>
	</head>
	<body>
		<p>Clicking select should highlight a column</p>
		<form runat="server">
			<asp:DataGrid id="DataGrid1" runat="server" PageSize="2" OnPageIndexChanged="changepage" AllowPaging="True" />
			
		</form>
	</body>
</html>
