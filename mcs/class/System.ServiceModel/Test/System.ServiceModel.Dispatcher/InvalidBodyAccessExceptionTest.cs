#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Dispatcher;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class InvalidBodyAccessExceptionTest
	{
		[Test]
		public void TestConstructor ()
		{
			FilterInvalidBodyAccessException e = new FilterInvalidBodyAccessException ();
			NavigatorInvalidBodyAccessException f = new NavigatorInvalidBodyAccessException ();
			// Don't expect Engligh.
			// Assert.AreEqual ("Not allowed to navigate to body.", e.Message);
			Assert.AreEqual (e.Message, f.Message);
		}
	}
}
#endif