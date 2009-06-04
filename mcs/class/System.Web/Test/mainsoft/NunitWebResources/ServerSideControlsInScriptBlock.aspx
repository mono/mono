<%@ Page Language="C#" %>
<html><head><title>Bug 508888</title></head>
<body>
<form runat="server">
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><script type="text/javascript">alert (escape("reporting/location?report=ViewsByDate&minDate=<asp:Literal id="minDate" runat="server"/>&maxDate=<asp:Literal id="maxDate" runat="server" />"));</script><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>

<script runat="server">

	void Page_Load(object sender, EventArgs e)
	{
		if (!Page.IsPostBack)
		{
            minDate.Text = "minDate";
            maxDate.Text = "maxDate";
		}
	}

</script>
</form>
</body></html>
