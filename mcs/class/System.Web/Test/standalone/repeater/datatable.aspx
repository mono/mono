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
		
		t.Columns.Add (new DataColumn ("Integer", typeof (int)));
		t.Columns.Add (new DataColumn ("String", typeof (string)));
		t.Columns.Add (new DataColumn ("Double", typeof (double)));

		DataSet ds = new DataSet ("ds");

		ds.Tables.Add (t);

		for (int i = 0; i < 3; i ++) {
			DataRow dr = t.NewRow ();

			dr [0] = i;
			dr [1] = "Blah blah blah " + i;
			dr [2] = i * Math.PI;
			t.Rows.Add (dr);
		}

		rep.DataSource = ds;
		rep.DataMember = "t";
		rep.DataBind ();		
	}
</script>
</head>
<body>
	<form runat="server">
		<asp:Repeater id="rep" runat="server">
			<HeaderTemplate>
				<h1>Hello, World</h1>
			</HeaderTemplate>
			<ItemTemplate>
				<%# DataBinder.Eval (Container.DataItem, "String") %>, 
				<%# DataBinder.Eval (Container.DataItem, "Double") %>
			</ItemTemplate>
			<FooterTemplate>
				<h2>Bye!</h2>
			</FooterTemplate>
			<SeparatorTemplate>
				<p>
			</SeparatorTemplate>
		</asp:Repeater>
	</form>
</body>
</html>
