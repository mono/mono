<%@ Page Language="C#" %>
<script runat="server">
	void Page_Load (object o, EventArgs e)
	{
		DataBind ();
	}
</script>

<html>
<body>
	<asp:datalist DataSource="<%# Request.Form %>" runat="server">
        	<ItemTemplate>
			<%# Container.DataItem %> -- <%# Request.Form [(string) Container.DataItem] %>
		</ItemTemplate>
	</asp:datalist>

	<form runat="server">
	      <asp:button Text="Go Here >" runat="server"/> 
	</form>
</body>
</html>
