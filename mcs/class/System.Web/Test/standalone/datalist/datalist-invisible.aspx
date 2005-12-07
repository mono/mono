<!-- based on bug #49024 -->

<%@ Page language="c#" %>
<%@ Import Namespace="System.Data" %>
<html>
	<head>
		<script runat="server">
		void Page_Load (object sender, EventArgs e)
		{
			if (IsPostBack)
				return;
			
			DataSet ds = new DataSet ();
			ds.ReadXml (new System.IO.StringReader (@"
<DataSet>
	<Stocks Company='Novell Inc.'     Symbol='NOVL' Price='6.14'   />
	<Stocks Company='Microsoft Corp.' Symbol='MSFT' Price='25.92'  />
	<Stocks Company='Google'          Symbol='GOOG' Price='291.60' />
</DataSet>
"));
			DataList1.DataSource = ds;
			DataList1.DataBind();
		}
	
		void BtnVisible_Click (object sender, EventArgs e)
		{
			DataList1.Visible = true;
		}

		void BtnInvisible_Click(object sender, System.EventArgs e)
		{
			DataList1.Visible = false;
		}
		</script>
	</head>
	<body>
		<p>Click invisible then click visible. The names should come back.</p>
		<form runat="server">
			<asp:DataList id="DataList1" runat="server">
				<ItemTemplate>
					<%# DataBinder.Eval (Container.DataItem, "Company") %>
				</ItemTemplate>
			</asp:DataList>
			<asp:button OnClick="BtnInvisible_Click" runat="server" Text="Invisible" />
			<asp:button OnClick="BtnVisible_Click" runat="server" Text="Visible" />
		</form>
	</body>
</html>
