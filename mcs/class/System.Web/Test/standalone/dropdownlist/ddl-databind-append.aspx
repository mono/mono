<!-- Inspired by bug #42246 -->

<%@ Page Language="C#" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Data" %>

<script runat="server">
	void Page_Load (object s, EventArgs e)
	{
		if (IsPostBack) {
			txt.Text = ddl.SelectedItem.Value;
			return;
		}
		
		DataTable t = new DataTable ("t");
		
		t.Columns.Add (new DataColumn ("Symbol", typeof (string)));
		t.Columns.Add (new DataColumn ("Company", typeof (string)));
		t.Columns.Add (new DataColumn ("Price", typeof (double)));

		DataSet ds = new DataSet ("ds");

		ds.Tables.Add (t);
		AddStock (t, "MSFT", "Microsoft Corp.", 25.81);
		AddStock (t, "NOVL", "Novell Inc.", 6.17);
		AddStock (t, "GOOG", "Google", 300.95);

		ddl.DataSource = ds;
		ddl.DataTextField = "Company";
		ddl.DataBind ();		
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

<asp:dropdownlist appenddatabounditems="true" id="ddl" runat="server">
    <asp:ListItem Text="(Select a Ticker)" Value="" />   
</asp:dropdownlist>

<asp:button id="lookup" Text="Lookup Ticker" runat="server" />

<asp:requiredfieldvalidator ControlToValidate="ddl" ErrorMessage="Please select a ticker" runat="server" />

<asp:label id="txt" runat="server" />
</form>
</body>
</html>
