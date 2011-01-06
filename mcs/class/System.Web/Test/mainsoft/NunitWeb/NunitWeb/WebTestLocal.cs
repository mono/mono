using System;

namespace MonoTests.SystemWeb.Framework
{
	public partial class WebTest
	{
		static partial void CopyResourcesLocal ()
		{
			Type myself = typeof (WebTest);
#if NET_2_0
			CopyPrefixedResources (myself, "App_GlobalResources/", "App_GlobalResources");
			CopyPrefixedResources (myself, "App_Code/", "App_Code");
#if DOTNET
			CopyResource (myself, "Web.config", "Web.config");
#else
#if NET_4_0
			CopyResource (myself, "Web.mono.config.4.0", "Web.config");
#else
			CopyResource (myself, "Web.mono.config", "Web.config");
#endif
#endif
#else
			CopyResource (myself, "Web.mono.config.1.1", "Web.config");
#endif
		}
	}
}
