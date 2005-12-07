<!-- bug 50246 -->
<%@ Page language="c#" %>
<html>
	<head>
		<script runat="server">
		void Page_Load (object sender, EventArgs e)
		{
			PopulateDataGrid();
		}
		
		void Button1_Click (object sender, System.EventArgs e)
		{			
			DataGrid1.PagerStyle.Visible = false;
		}
		
		void PopulateDataGrid()
		{
			ArrayList al = new ArrayList ();
			al.Add("Item 1");
			al.Add("Item 2");
			al.Add("Item 3");
			al.Add("Item 4");
			
			DataGrid1.DataSource = al;
			DataGrid1.DataBind ();
		}
		
		void DataGrid1_PageIndexChanged (object source, DataGridPageChangedEventArgs e)
		{
			DataGrid1.CurrentPageIndex = e.NewPageIndex;
			PopulateDataGrid ();
		}
		</script>
	</head>
	<body>
		<form runat="server">
			<asp:datagrid id="DataGrid1" runat="server" AllowPaging="True" OnPageIndexChanged="DataGrid1_PageIndexChanged"
				PageSize="2">
				<PagerStyle BackColor=Yellow />
			</asp:datagrid>
			<asp:Button OnClick="Button1_Click" runat="server" Text="Assign Pager Style" />
		</form>
	</body>
</html>
