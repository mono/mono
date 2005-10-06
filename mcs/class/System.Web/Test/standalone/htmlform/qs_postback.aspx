<%@ Page Language="C#" AutoEventWireup="True" %>

<html>
<head>
<script runat="server">
	void Page_Load (object o, EventArgs e)
	{
		DataBind ();
	}
</script>
</head>
<body>
	<p>
	Instructions: put a query string (ie ...aspx?a=b) in the
	location bar. Then click the button. You should get the same
	query string on the resulting page.
	</p>

	<form runat="server">
		<asp:button runat="server" Text="Click Me!"/>
		<br>
		Your query string:<br>
		<asp:DataList id="DataList1" DataSource="<%# Request.QueryString %>" runat="server">

        	<ItemTemplate>
			<%# Container.DataItem %> -- <%# Request.QueryString [(string) Container.DataItem] %>
		</ItemTemplate>

		</asp:datalist>
	</form>
</body>
</html>
