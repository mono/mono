<!-- Based on bug #43823 -->

<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data" %>
	
<script runat="server">
	void Page_Load (object o, EventArgs e)
	{
		if (IsPostBack)
			return;
	
		
		DataTable t = new DataTable ("t");
		
		t.Columns.Add (new DataColumn ("Symbol", typeof (string)));
		t.Columns.Add (new DataColumn ("Company", typeof (string)));
		t.Columns.Add (new DataColumn ("Price", typeof (double)));

		DataSet ds = new DataSet ("ds");

		ds.Tables.Add (t);
		AddStock (t, "MSFT", "Microsoft Corp.", 25.81);
		AddStock (t, "NOVL", "Novell Inc.", 6.17);
		AddStock (t, "GOOG", "Google", 300.95);

		dg.DataSource = ds;
		dg.DataBind ();
	}

	void AddStock (DataTable dt, string symbol, string co, double price)
	{
		DataRow dr = dt.NewRow ();
		dr [0] = symbol;
		dr [1] = co;
		dr [2] = price;
		dt.Rows.Add (dr);
	}
</script>

<html>
<body>
<form runat="server">
<asp:datagrid
	ID="dg"
	CellPadding="10"
	HeaderStyle-Font-Name="Arial"
	HeaderStyle-Font-Bold="True"
	HeaderStyle-BackColor="lightyellow"
	ItemStyle-Font-Name="Arial"
	ItemStyle-Font-Size="10pt"
	AlternatingItemStyle-BackColor="lightblue"
	runat="server" />
</form>
</body>
</html>
