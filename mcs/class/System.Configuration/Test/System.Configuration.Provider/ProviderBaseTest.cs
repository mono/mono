//
// System.Configuration.Provider.ProviderBaseTest.cs - Unit tests
// for System.Configuration.Provider.ProviderBase.
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2007 Gert Driesen
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
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;

using NUnit.Framework;

namespace MonoTests.System.Configuration.Provider
{
	[TestFixture]
	public class ProviderBaseTest
	{
		[Test]
		public void Initialize ()
		{
			MockProvider provider = new MockProvider ();
			provider.Initialize ("Mono", (NameValueCollection) null);
			Assert.IsNotNull (provider.Description, "#A1");
			Assert.AreEqual ("Mono", provider.Description, "#A2");
			Assert.IsNotNull (provider.Name, "#A3");
			Assert.AreEqual ("Mono", provider.Name, "#A4");

			provider = new MockProvider ();
			provider.Initialize (" ", (NameValueCollection) null);
			Assert.IsNotNull (provider.Description, "#B1");
			Assert.AreEqual (" ", provider.Description, "#B2");
			Assert.IsNotNull (provider.Name, "#B3");
			Assert.AreEqual (" ", provider.Name, "#B4");

			NameValueCollection config = new NameValueCollection ();
			config ["name"] = "Novell";
			config ["description"] = "DESC";
			config ["foo"] = "FOO"; 

			provider = new MockProvider ();
			provider.Initialize ("Mono", config);
			Assert.IsNotNull (provider.Description, "#C1");
			Assert.AreEqual ("DESC", provider.Description, "#C2");
			Assert.IsNotNull (provider.Name, "#C3");
			Assert.AreEqual ("Mono", provider.Name, "#C4");
			Assert.IsTrue (ContainsKey (config, "name"), "#C5");
			Assert.IsFalse (ContainsKey (config, "description"), "#C6");
			Assert.IsTrue (ContainsKey (config, "foo"), "#C7");

			config = new NameValueCollection ();
			config ["description"] = null;

			provider = new MockProvider ();
			provider.Initialize ("Mono", config);
			Assert.IsNotNull (provider.Description, "#D1");
			Assert.AreEqual ("Mono", provider.Description, "#D2");
			Assert.IsNotNull (provider.Name, "#D3");
			Assert.AreEqual ("Mono", provider.Name, "#D4");
			Assert.IsFalse (ContainsKey (config, "description"), "#D5");

			config = new NameValueCollection ();
			config ["description"] = string.Empty;

			provider = new MockProvider ();
			provider.Initialize ("Mono", config);
			Assert.IsNotNull (provider.Description, "#E1");
			Assert.AreEqual ("Mono", provider.Description, "#E2");
			Assert.IsNotNull (provider.Name, "#E3");
			Assert.AreEqual ("Mono", provider.Name, "#E4");
			Assert.IsFalse (ContainsKey (config, "description"), "#E5");

			config = new NameValueCollection ();
			config ["description"] = " ";

			provider = new MockProvider ();
			provider.Initialize ("Mono", config);
			Assert.IsNotNull (provider.Description, "#F1");
			Assert.AreEqual (" ", provider.Description, "#F2");
			Assert.IsNotNull (provider.Name, "#F3");
			Assert.AreEqual ("Mono", provider.Name, "#F4");
			Assert.IsFalse (ContainsKey (config, "description"), "#F5");
		}

		[Test]
		public void Initialize_AlreadyInitialized ()
		{
			MockProvider provider = new MockProvider ();
			provider.Initialize ("Mono", (NameValueCollection) null);
			try {
				provider.Initialize ("Mono", (NameValueCollection) null);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Initialize_Name_Null ()
		{
			MockProvider provider = new MockProvider ();
			try {
				provider.Initialize ((string) null, new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Initialize_Name_Empty ()
		{
			MockProvider provider = new MockProvider ();
			try {
				provider.Initialize (string.Empty, new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		static bool ContainsKey (NameValueCollection collection, string searchKey)
		{
			foreach (string key in collection)
				if (key == searchKey)
					return true;
			return false;
		}

		class MockProvider : ProviderBase
		{
		}
	}
}

#endif
