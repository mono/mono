<script language="C#" runat="server">

void Application_Start(Object sender, EventArgs e)
{
        // Initialize user count property
        Application["UserCount"] = 0;
}
    
public void AnonymousIdentification_Creating(Object sender, AnonymousIdentificationEventArgs e)
{
	// Change the anonymous id
	e.AnonymousID = "mysite.com_Anonymous_User_" + DateTime.Now.Ticks;

	// Increment count of unique anonymous users
	Application["UserCount"] = Int32.Parse(Application["UserCount"].ToString()) + 1;
}

</script>
