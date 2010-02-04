<%@ Application Language="C#" %>
<script RunAt="server">
	public override string GetOutputCacheProviderName (HttpContext context)
	{
		if (context == null)
			throw new ArgumentNullException ("context");

		HttpRequest req = context.Request;
		if (req == null)
			throw new InvalidOperationException ("No request found.");

		switch (req.QueryString["ocp"]) {
			case "InMemory":
				return "TestInMemoryProvider";

			case "AnotherInMemory":
				return "TestAnotherInMemoryProvider";

			case "invalid":
				return "NoSuchProviderFound";

			default:
				return base.GetOutputCacheProviderName (context);
		}
	}
</script>
