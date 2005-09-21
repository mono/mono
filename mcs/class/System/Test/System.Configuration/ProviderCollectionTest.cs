//
// System.Configuration.ProviderCollectionTest.cs - Unit tests for
// System.Configuration.ProviderCollection.
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

namespace MonoTests.System.Configuration {

	class TestProvider : SettingsProvider {
		public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context,
										   SettingsPropertyCollection collection)
		{
			throw new NotImplementedException ();
		}

		public override void SetPropertyValues (SettingsContext context,
							SettingsPropertyValueCollection collection)
		{
			throw new NotImplementedException ();
		}

		public override string ApplicationName {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	}

	class TestProviderBase : ProviderBase {
	}

	[TestFixture]
	public class ProviderCollectionTest {

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Add_duplicate ()
		{
			ProviderCollection col = new ProviderCollection();
			TestProvider provider;

			provider = new TestProvider();
			provider.Initialize ("test", null);


			col.Add (provider);
			col.Add (provider);
		}

		[Test]
		public void Add_providerbase ()
		{
			ProviderCollection col = new ProviderCollection();
			TestProviderBase provider;

			provider = new TestProviderBase();
			provider.Initialize ("test", null);

			col.Add (provider);

			Assert.AreEqual (provider, col["test"], "A1");
		}

		[Test]
		public void Get_nonexistant ()
		{
			ProviderCollection col = new ProviderCollection();
			TestProvider provider;

			provider = new TestProvider();
			provider.Initialize ("test", null);


			col.Add (provider);

			Assert.AreEqual (provider, col["test"], "A1");
			Assert.IsNull (col["test2"], "A2");
		}

		[Test]
		public void Ctor_2 ()
		{
			SettingsProperty q = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   10,
								   SettingsSerializeAs.Binary,
								   new SettingsAttributeDictionary(),
								   true,
								   false);

			SettingsProperty p = new SettingsProperty (q);

			Assert.AreEqual ("property", p.Name, "A1");
			Assert.AreEqual (typeof (int), p.PropertyType, "A2");
			Assert.AreEqual (null, p.Provider, "A3");
			Assert.AreEqual (10, (int)p.DefaultValue, "A4");
			Assert.AreEqual (SettingsSerializeAs.Binary, p.SerializeAs, "A5");
			Assert.IsNotNull (p.Attributes, "A6");
			Assert.IsTrue (p.ThrowOnErrorDeserializing, "A7");
			Assert.IsFalse (p.ThrowOnErrorSerializing, "A8");
			Assert.IsTrue (p.IsReadOnly, "A9");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_2_ArgNull ()
		{
			/* same as above, but a null
			 * SettingsAttributeDictionary, which causes a
			 * ANE in the ctor. */
			SettingsProperty q = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   10,
								   SettingsSerializeAs.Binary,
								   null,
								   true,
								   false);

			SettingsProperty p = new SettingsProperty (q);
		}

		[Test]
		public void Ctor_3 ()
		{
			SettingsProperty p = new SettingsProperty ("property");

			Assert.AreEqual ("property", p.Name, "A1");
			Assert.AreEqual (null, p.PropertyType, "A2");
			Assert.AreEqual (null, p.Provider, "A3");
			Assert.AreEqual (null, p.DefaultValue, "A4");
			Assert.AreEqual (SettingsSerializeAs.String, p.SerializeAs, "A5");
			Assert.IsNotNull (p.Attributes, "A6");
			Assert.IsFalse (p.ThrowOnErrorDeserializing, "A7");
			Assert.IsFalse (p.ThrowOnErrorSerializing, "A8");
			Assert.IsFalse (p.IsReadOnly, "A9");
		}

	}

}

#endif
