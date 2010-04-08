using System;
using System.Runtime.Caching;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Caching
{
	[TestFixture]
	public class HostFileChangeMonitorTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullFilePaths ()
		{
			new HostFileChangeMonitor (null);
		}
	}
}
