<!-- Inspired by bug 48671, should do autopostback -->

<%@ Page language="c#" %>
<%@ Import Namespace="System.Data" %>
<html>
	<head>
		<script runat="server" language="C#">
		void Page_Load (object sender, EventArgs e)
		{
			if (IsPostBack) {
				txt.Text = "Postback";
				return;
			}

			DataSet ds = new DataSet();
			ds.ReadXml (new System.IO.StringReader (@"
<DataSet>
	<Stocks Company='Novell Inc.'     Symbol='NOVL' Price='6.14'   />
	<Stocks Company='Microsoft Corp.' Symbol='MSFT' Price='25.92'  />
	<Stocks Company='Google'          Symbol='GOOG' Price='291.60' />
</DataSet>
"));
			l.DataSource = ds;
			l.DataValueField = "Symbol";
			l.DataTextField = "Company";
			l.DataBind (); 
		}
		</script>
	</head>
	<body>
		<asp:label id="txt" runat="server" />
		<form runat="server">
			<asp:listbox selectionmode="multiple" autopostback="true" id="l" runat="server" />
		</form>
	</body>
</html>
