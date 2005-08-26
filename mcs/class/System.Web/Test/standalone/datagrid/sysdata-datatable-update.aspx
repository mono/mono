<!-- Based on bug #43823. Also tests bug #48212 (using font-names) -->

<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data" %>
	
<script runat="server">
	DataTable t;

	void Page_Load (object o, EventArgs e)
	{
		if (IsPostBack) {
			t = (DataTable) ViewState ["dt"];
			return;
		}
		
		t = new DataTable ("t");
		
		t.Columns.Add (new DataColumn ("Symbol", typeof (string)));
		t.Columns.Add (new DataColumn ("Company", typeof (string)));
		t.Columns.Add (new DataColumn ("Price", typeof (double)));

		DataColumn[] keys = new DataColumn[1];
		keys[0] = t.Columns[0];
		t.PrimaryKey = keys;

		AddStock (t, "MSFT", "Microsoft Corp.", 25.81);
		AddStock (t, "NOVL", "Novell Inc.", 6.17);
		AddStock (t, "GOOG", "Google", 300.95);

		BindGrid ();
	}

	void BindGrid ()
	{
		// Don't ever do this in real code. It bloats the viewstate
		ViewState ["dt"] = t;
		dg.DataSource = t;
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

	void edit (Object sender, DataGridCommandEventArgs e) 
	{
		
		dg.EditItemIndex = e.Item.ItemIndex;
		BindGrid ();
		
	}
 
	void cancel (Object sender, DataGridCommandEventArgs e) 
	{
		dg.EditItemIndex = -1;
		BindGrid ();
	}
 
	void update (Object sender, DataGridCommandEventArgs e) 
	{
		TextBox txtprice = (TextBox) e.Item.FindControl ("txtprice");
		DataRow dr = t.Rows.Find (dg.DataKeys [e.Item.ItemIndex]);

		dr [2] = Convert.ToDouble (txtprice.Text);
		dg.EditItemIndex = -1;
		BindGrid ();	
	}
</script>

<html>
<body>
<form runat="server">

<asp:DataGrid
  ID="dg"
  Font-Names="verdana, arial, helvetica" 
  OnEditCommand="edit"
  OnUpdateCommand="update"
  OnCancelCommand="cancel"
  DataKeyField="Symbol"
  AutoGenerateColumns="False"
  CellPadding="10"
  HeaderStyle-BackColor="Salmon"
  runat="server">
<Columns>
  <asp:BoundColumn
    HeaderText="Symbol"
    DataField="Symbol"
    ReadOnly="True" />
  <asp:BoundColumn
    HeaderText="Company"
    DataField="Company"
    ReadOnly="True" />
  <asp:TemplateColumn>
  <HeaderTemplate>
    Price
  </HeaderTemplate>
  <ItemTemplate>
     <%# DataBinder.Eval(Container.DataItem, "Price" ) %>
  </ItemTemplate>
  <EditItemTemplate>
    <asp:TextBox
      ID="txtprice"
      Text='<%# DataBinder.Eval(Container.DataItem, "Price" ) %>'
      runat="server" />
  </EditItemTemplate>	
  </asp:TemplateColumn>
  <asp:EditCommandColumn
    EditText="Edit"
    UpdateText="Update"
    CancelText="Cancel" />
</Columns>
</asp:DataGrid>
</form>
</body>
</html>
