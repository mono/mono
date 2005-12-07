<!--
In this case, we have a checkbox inside a repeater control, which is a
naming container. We need to make sure to use the correct attribute on
the label element so that the browser knows how to hook up the label
and the widget.
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
</script>
</head>
<body>
	<asp:Label id="lbl1" runat="server" />
	<form runat="server">
		<asp:Repeater id="rep" runat="server">
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
					<td><asp:checkbox id="buy" runat="server" text="Buy"/></td>
				</tr>
			</ItemTemplate>
			<FooterTemplate>
				</table>
			</FooterTemplate>
		</asp:Repeater>
	</form>
</body>
</html>
