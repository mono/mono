using System;

namespace MonoTests.SystemWeb.Framework
{
	public partial class WebTest
	{
		static partial void CopyResourcesLocal ()
		{
			Type myself = typeof (WebTest);
#if !DOTNET
			CopyResource (myself, "Web.mono.config", "Web.config");
#endif
#if NET_4_0
			CopyResource (myself, "profile.config.4.0", "profile.config");
#else
			CopyResource (myself, "profile.config.2.0", "profile.config");
#endif
		}
	}
}
