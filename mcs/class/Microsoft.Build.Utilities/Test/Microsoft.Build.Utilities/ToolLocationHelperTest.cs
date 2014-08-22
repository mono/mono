#if NET_4_0
using System;
using NUnit.Framework;
using Microsoft.Build.Utilities;

namespace MonoTests.Microsoft.Build.Utilities
{
	[TestFixture]
	public class ToolLocationHelperTest
	{
		[Test]
		[Category ("NotWorking")] // this test needs extra xbuild testing settings, as the target framework path is different.
		public void GetPathToStandardLibraries ()
		{
			Assert.IsTrue (!string.IsNullOrEmpty (ToolLocationHelper.GetPathToStandardLibraries (".NETFramework", "v4.0", null)), "std path");
			Assert.IsTrue (!string.IsNullOrEmpty (ToolLocationHelper.GetPathToStandardLibraries (".NETFramework", "v4.0", string.Empty)), "empty Profile path");
		}
	}
}

#endif
