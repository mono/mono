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
		}
	}
}
