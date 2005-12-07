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

		rep.DataSource = ds;
		rep.DataMember = "t";
		rep.DataBind ();		
	}

	void AddStock (DataTable dt, string symbol, string co, double price)
	{
		DataRow dr = dt.NewRow ();
		dr [0] = symbol;
		dr [1] = co;
		dr [2] = price;
		dt.Rows.Add (dr);
	}

	void ItemCommand (object sender, RepeaterCommandEventArgs e)
	{
		lbl1.Text = String.Format ("You bought {0}", e.CommandArgument);
	}
</script>
</head>
<body>
	<asp:Label id="lbl1" runat="server" />
	<form runat="server">
		<asp:Repeater id="rep" OnItemCommand="ItemCommand" runat="server">
			<HeaderTemplate>
				<table>
					<thead>
						<tr>
						<td>Stock</td>
						<td>Company</td>
						<td>Price</td>
						<td>Buy</td>
						</tr>
					</thead>
			</HeaderTemplate>
			<ItemTemplate>
				<tr>
					<td><%# DataBinder.Eval (Container.DataItem, "Symbol") %></td> 
					<td><%# DataBinder.Eval (Container.DataItem, "Company") %></td>
					<td><%# DataBinder.Eval (Container.DataItem, "Price") %></td>
					<td><asp:LinkButton Text="Buy" runat="server" CommandArgument='<%# DataBinder.Eval (Container.DataItem, "Symbol") %>' /></td>
				</tr>
			</ItemTemplate>
			<FooterTemplate>
				</table>
			</FooterTemplate>
		</asp:Repeater>
	</form>
</body>
</html>
