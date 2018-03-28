
using System;

internal partial class Interop
{
	internal partial class mincore
	{
		static mincore()
		{
			if (!Environment.IsRunningOnWindows)
				throw new PlatformNotSupportedException();
		}
	}
}
