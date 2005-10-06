<!-- Inspired by bug 49020 -->
<%@ Page language="c#" Debug="true" %>
<%@ Import Namespace="System.Data" %>
<html>
	<head>
		<script runat="server">
		DataSet ds;
		private void Page_Load (object sender, EventArgs e)
		{
			PopulateList();
		}
		
		private void PopulateList()
		{
			if (IsPostBack) {
				ds = (DataSet) ViewState ["ds"];
				return;
			} else {
				ds = new DataSet ();
				ds.ReadXml (new System.IO.StringReader (@"
<DataSet>
	<Stocks Company='Novell Inc.'     Symbol='NOVL' Price='6.14'   />
	<Stocks Company='Microsoft Corp.' Symbol='MSFT' Price='25.92'  />
	<Stocks Company='Google'          Symbol='GOOG' Price='291.60' />
</DataSet>
"));
				ViewState ["ds"] = ds;
			}

			DataList1.GridLines = GridLines.Both;
			DataList1.DataSource = ds;
			DataList1.DataBind();
		}
	
		private void EditCommand(object source, System.Web.UI.WebControls.DataListCommandEventArgs e)
		{
			DataList1.EditItemIndex = e.Item.ItemIndex;
			DataList1.DataSource = ds;
			DataList1.DataBind();	
		}
		
		private void UpdateCommand(object source, System.Web.UI.WebControls.DataListCommandEventArgs e)
		{
			
			string name = ((TextBox)e.Item.FindControl("edit_name")).Text;
			ds.Tables[0].Rows [DataList1.EditItemIndex]["Company"] = name;
			DataList1.EditItemIndex = -1;
			ViewState ["ds"] = ds;
			DataList1.DataSource = ds;
			DataList1.DataBind();
		}
		</script>
	</head>
	<body>
		<form runat="server">
			<asp:datalist
				runat="server"
				id="DataList1"
				OnEditCommand="EditCommand"
				OnUpdateCommand="UpdateCommand"
				RepeatColumns="2"
				RepeatDirection="vertical">
				<ItemTemplate>
					<asp:label runat="server" Text='<%# DataBinder.Eval (Container.DataItem, "Company") %>' />
					<asp:LinkButton Runat="server" CommandName="Edit" Text="Edit" />
				</ItemTemplate>
				<EditItemTemplate>
						<asp:textbox id="edit_name" text='<%# DataBinder.Eval(Container.DataItem, "Company") %>' runat="server" />
						<asp:linkbutton runat="server" commandname="Update" text="Update" />
				</EditItemTemplate>
			</asp:datalist>
		</form>
	</body>
</html>
