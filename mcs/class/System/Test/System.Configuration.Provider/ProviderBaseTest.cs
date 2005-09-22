//
// System.Configuration.Provider.ProviderBaseTest.cs - Unit tests for
// System.Configuration.Provider.ProviderBase.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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
using System.Text;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Configuration.Provider {

	class TestProviderBase : ProviderBase {
	}

	[TestFixture]
	public class ProviderBaseTest {

		[Test]
		public void Properties ()
		{
			/* simulate what should happen with the following <provider> line:
			   <provider name="FooProvider" description="Provider for foos" />
			 */

			NameValueCollection extra_attrs = new NameValueCollection();
			extra_attrs.Add ("description", "Provider for foos");

			TestProviderBase test = new TestProviderBase ();

			test.Initialize ("FooProvider", extra_attrs);

			Assert.AreEqual ("FooProvider", test.Name, "A1");
			Assert.AreEqual ("Provider for foos", test.Description, "A2");

			/* simulate what should happen with the following <provider> line:
			   <provider name="FooProvider" /> */

			extra_attrs = new NameValueCollection();
			test = new TestProviderBase ();

			test.Initialize ("FooProvider", extra_attrs);

			Assert.AreEqual ("FooProvider", test.Name, "A3");
			Assert.AreEqual ("FooProvider", test.Description, "A4");
		}
	}

}

#endif
