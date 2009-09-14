<%@ Application Language="C#" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="System.Web.DynamicData" %>
<%@ Import Namespace="MonoTests.DataObjects" %>
<%@ Import Namespace="MonoTests.DataSource" %>
<%@ Import Namespace="MonoTests.ModelProviders" %>
<%@ Import Namespace="MonoTests.Common" %>

<script RunAt="server">

    void RegisterRoutes (RouteCollection routes) {
        var provider = new DynamicDataContainerModelProvider<EmployeesDataContext> ();
        provider.ResolveAssociations ();
        Utils.RegisterContext (provider, new ContextConfiguration () { ScaffoldAllTables = true }, true);
        
        var provider2 = new DynamicDataContainerModelProvider<TestDataContext3> ();
        provider2.ResolveAssociations ();
        Utils.RegisterContext (provider2, new ContextConfiguration () { ScaffoldAllTables = true }, true);
        routes.Add (new DynamicDataRoute ("{table}/{action}.aspx") {
            Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
            Model = MetaModel.Default
        });
    }

    void Application_Start (object sender, EventArgs e) {
        RegisterRoutes (RouteTable.Routes);
    }
</script>

