// created on 7/21/2001 at 2:36 PM
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Collections.Specialized {

	[TestFixture]
        public class NameValueCollectionTest {

		[Test]
		public void GetValues ()
		{
			NameValueCollection col = new NameValueCollection ();
			col.Add ("foo1", "bar1");
			Assertion.AssertEquals ("#1", null, col.GetValues (null));
			Assertion.AssertEquals ("#2", null, col.GetValues (""));
			Assertion.AssertEquals ("#3", null, col.GetValues ("NotExistent"));
		}
	}
}

