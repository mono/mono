<%@ Page Language="C#" %>
<script runat="server">
	protected void Page_Load (object sender, EventArgs a)
	{
		grid.DataSource = new string[] {"one"};		
		grid.DataBind ();
	}

	protected string BindSomeText(object someObj) { return "Test"; }
</script>
<html><head><title>Bug 493639</title></head>
<body>
<form runat="server">
	<asp:GridView runat="server" id="grid" AutoGenerateColumns="false">
	<columns>
		<asp:TemplateField>
			<ItemTemplate><%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:Label runat="server" ID="lblTest" Text="<%# BindSomeText(Container.DataItem) %>" /><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %></ItemTemplate>
		</asp:TemplateField>
	</columns>
	</asp:GridView>
</form>
</body>
</html>
