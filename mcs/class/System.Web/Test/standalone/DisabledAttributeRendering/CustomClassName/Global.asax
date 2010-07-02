<%@ Application Language="C#" %>
<script RunAt="server">
	void Application_Start (object sender, EventArgs e)
	{
		WebControl.DisabledCssClass = "MyDisabledControlClass";
	}   
</script>
