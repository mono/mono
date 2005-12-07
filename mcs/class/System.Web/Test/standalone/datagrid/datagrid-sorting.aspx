<!-- bug 51424 -->

<%@ Page language="c#" %>
<%@ Import Namespace="System.Data" %>
<html>
	<body>
		<form runat="server">
			<asp:DataGrid id="DataGrid1" runat="server" />
		</form>
	</body>
	<script language="c#" runat="server">
		void Page_Load(object sender, System.EventArgs e)
		{
				DataGrid1.AllowSorting = true;
				if (!IsPostBack)
				{
					DataGrid1.DataSource = createDataTable();
					DataGrid1.DataBind();
				}
				this.DataGrid1.SortCommand += new System.Web.UI.WebControls.DataGridSortCommandEventHandler(this.DataGrid1_SortCommand);
		}
		void DataGrid1_SortCommand(object sender, System.Web.UI.WebControls.DataGridSortCommandEventArgs e)
		{
			DataView dv = new DataView();
			dv.Table = createDataTable();			
			dv.Sort = e.SortExpression;       
			DataGrid1.DataSource = dv;
			DataGrid1.DataBind();
		}
		System.Data.DataTable createDataTable()
		{
			System.Data.DataTable dt = new System.Data.DataTable("Customers");
			System.Data.DataColumn dc = dt.Columns.Add("ID",typeof(int));
			dc.AllowDBNull = false;
			dc.AutoIncrement = true;
			dc.AutoIncrementSeed = 1;
			dc.AutoIncrementStep = 1;
			dc.Unique = true;

			dt.PrimaryKey = new System.Data.DataColumn[] {dc};

	        	dc = dt.Columns.Add("Name", typeof(String));
			dc.MaxLength = 14;
			dc.DefaultValue = "nobody";
			dc = dt.Columns.Add("Company", typeof(String));
			dc.MaxLength = 14;
			dc.DefaultValue = "nonexistent";

			ArrayList arr = createArrayList();
			IEnumerator items = arr.GetEnumerator();
            		items.Reset();
            		while(items.MoveNext())
			{
                		DataRow dr = dt.NewRow();
                		dr["Name"] = "n_" + "_" + items.Current;
                		dr["Company"] = "c_" + "_" + items.Current;
				dt.Rows.Add(dr);
			}
			return dt;
		}

		ArrayList createArrayList()
		{
			ArrayList arr = new ArrayList();
			arr.Add("One");
			arr.Add("Two");
			arr.Add("Three");
			arr.Add("Four");
			arr.Add("Five");
			return arr;
		}
	</script>
</html>
