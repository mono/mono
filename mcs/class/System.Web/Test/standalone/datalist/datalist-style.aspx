<!--
This page should render a table with cellpadding="3" and cellspacing="1"
-->

<%@ Page Language="C#" AutoEventWireup="True" %>
<%@ Import Namespace="System.Data" %>

<html>
<head>
<script runat="server">
	void Page_Load (object s, EventArgs e)
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

		dl.DataSource = ds;
		dl.DataMember = "t";
		dl.DataBind ();		
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
</head>
<body>
	<asp:datalist id="dl" runat="server" Cellpadding="3" cellspacing="1" width="100%">
		<ItemTemplate>
			<%# DataBinder.Eval (Container.DataItem, "Symbol") %>, 
			<%# DataBinder.Eval (Container.DataItem, "Company") %>,
			<%# DataBinder.Eval (Container.DataItem, "Price") %>
		</ItemTemplate>
	</asp:datalist>
</body>
</html>
