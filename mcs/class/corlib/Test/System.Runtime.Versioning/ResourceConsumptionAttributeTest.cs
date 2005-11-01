//
// ResourceConsumptionAttributeTest.cs - Unit tests for 
//	System.Runtime.Versioning.ResourceConsumptionAttribute
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.Runtime.Versioning;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Versioning {

	[TestFixture]
	public class ResourceConsumptionAttributeTest {

		[Test]
		public void Constructor1 ()
		{
			ResourceConsumptionAttribute rca = null;
			Array values = Enum.GetValues (typeof (ResourceScope));
			foreach (ResourceScope resource in values) {
				rca = new ResourceConsumptionAttribute (resource);
				string id = String.Format ("[{0}]", resource);
				Assert.AreEqual (resource, rca.ResourceScope, "ResourceScope-" + id);
				Assert.AreEqual (resource, rca.ConsumptionScope, "ConsumptionScope-" + id);
			}
		}

		[Test]
		public void Constructor2 ()
		{
			ResourceConsumptionAttribute rca = null;
			Array values = Enum.GetValues (typeof (ResourceScope));
			foreach (ResourceScope resource in values) {
				foreach (ResourceScope consumption in values) {
					rca = new ResourceConsumptionAttribute (resource, consumption);
					string id = String.Format ("[{0}-{1}]", resource, consumption);
					Assert.AreEqual (resource, rca.ResourceScope, "ResourceScope-" + id);
					Assert.AreEqual (consumption, rca.ConsumptionScope, "ConsumptionScope-" + id);
				}
			}
		}

		[Test]
		public void InvalidResourceScope1 ()
		{
			ResourceScope bad = (ResourceScope) Int32.MinValue;
			ResourceConsumptionAttribute rca = new ResourceConsumptionAttribute (bad);
			Assert.AreEqual (bad, rca.ResourceScope, "ResourceScope");
			Assert.AreEqual (bad, rca.ConsumptionScope, "ConsumptionScope");
		}

		[Test]
		public void InvalidResourceScope2 ()
		{
			ResourceScope bad = (ResourceScope) Int32.MinValue;
			ResourceConsumptionAttribute rca = new ResourceConsumptionAttribute (ResourceScope.None, bad);
			Assert.AreEqual (ResourceScope.None, rca.ResourceScope, "ResourceScope");
			Assert.AreEqual (bad, rca.ConsumptionScope, "ConsumptionScope");
		}
	}
}

#endif
