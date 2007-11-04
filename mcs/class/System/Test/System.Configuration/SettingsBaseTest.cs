//
// SettingsBaseTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

//#define SPEW

#if NET_2_0

using System;
using System.Text;
using System.Configuration;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Configuration
{
	[TestFixture]
	public class SettingsBaseTest
	{
#if TARGET_JVM
		class CustomerException : Exception	{ }
#endif
		class MySettings : SettingsBase
		{
			[UserScopedSetting] // ignored in non-ApplicationSettingsBase
			public int Foo {
				get { return (int) this ["Foo"]; }
				set { this ["Foo"] = value; }
			}

			[UserScopedSetting] // ignored in non-ApplicationSettingsBase
			[DefaultSettingValue ("20")]
			public int Bar {
				get { return (int) this ["Bar"]; }
				set { this ["Bar"] = value; }
			}
		}

		class MySettings2 : SettingsBase
		{
			int foo;

			[UserScopedSetting] // ignored in non-ApplicationSettingsBase
			public int Foo {
				get { return (int) this ["Foo"]; }
				set { this ["Foo"] = value; }
			}

			public override SettingsPropertyCollection Properties {
				get { return null; }
			}

			public SettingsPropertyCollection BaseProperties {
				get { return base.Properties; }
			}
		}

		[Test]
		public void PropertyDefaults ()
		{
			MySettings s = new MySettings ();
			Assert.IsNull (s.Properties, "#1");
			Assert.IsNull (s.Providers, "#2");
			Assert.IsNull (s.Context, "#3");
			Assert.AreEqual (0, s.PropertyValues.Count, "#4");
			Assert.IsNull (s.Properties, "#5");
			Assert.IsNull (s.Providers, "#6");
			Assert.IsNull (s.Context, "#7");
			s.Initialize (s.Context, s.Properties, s.Providers);
		}

		[Test]
		public void PropertiesOverriden ()
		{
			MySettings2 s = new MySettings2 ();
			s.Initialize (s.Context, new SettingsPropertyCollection (), s.Providers);
			Assert.IsNull (s.Properties, "#1");
			Assert.IsNotNull (s.BaseProperties, "#2");
			Assert.AreEqual (0, s.PropertyValues.Count, "#3");
		}

		[Test]
		public void PropertyValuesInstance ()
		{
			SettingsPropertyCollection props = new SettingsPropertyCollection ();
			SettingsProviderCollection provs = new SettingsProviderCollection ();

			MyProvider p = new MyProvider ();
			MySettings s = new MySettings ();

			props.Add (new SettingsProperty ("Foo", typeof (string), p, false, 10, SettingsSerializeAs.String, null, true, true));
			provs.Add (p);

			s.Initialize (new SettingsContext (), props, provs);
			Assert.AreEqual (s.PropertyValues, s.PropertyValues);
		}

		[Test]
		public void PropertyValuesUninitialized ()
		{
			MySettings s = new MySettings ();
			s.Initialize (new SettingsContext (), new SettingsPropertyCollection (), new SettingsProviderCollection ());
			s.Properties.Add (new SettingsProperty ("Foo"));
			// values are filled only at initialization phase.
			Assert.AreEqual (0, s.PropertyValues.Count, "#1");
		}

		[Test]
		public void PropertyValuesInitialized ()
		{
			SettingsPropertyCollection props = new SettingsPropertyCollection ();
			SettingsProviderCollection provs = new SettingsProviderCollection ();

			MyProvider p = new MyProvider ();
			MySettings s = new MySettings ();
			int i;

			try {
				i = s.Foo;
				Assert.Fail ("#1-2");
			} catch (SettingsPropertyNotFoundException) {
			}

			s.Initialize (new SettingsContext (), props, provs);
			Assert.AreEqual (0, s.PropertyValues.Count, "#2-1");
			Assert.AreEqual (0, s.Context.Count, "#2-2");

			props.Add (new SettingsProperty ("Foo", typeof (int), p, false, 10, SettingsSerializeAs.String, null, true, true));
			// initialize w/o the provider
			s.Initialize (new SettingsContext (), props, provs);
			Assert.AreEqual (0, s.PropertyValues.Count, "#3-0");
			Assert.AreEqual (100, s.Foo, "#3-1");
			// ... !!!
			Assert.AreEqual (1, s.PropertyValues.Count, "#3-2");
			SettingsPropertyValue v = s.PropertyValues ["Foo"];
			Assert.AreEqual (100, v.PropertyValue, "#3-3");
			Assert.AreEqual (0, s.Context.Count, "#3-4");

			// initialize w/ the provider
			provs.Add (p);
			provs.Add (new MyProvider2 ("Bar", 25));
			props.Add (new SettingsProperty ("Bar", typeof (int), provs ["MyProvider2"], false, 10, SettingsSerializeAs.String, null, true, true));
			s.Initialize (new SettingsContext (), props, provs);
			Assert.AreEqual (1, s.PropertyValues.Count, "#4-1");
			Assert.AreEqual (100, s.Foo, "#4-2");
			Assert.AreEqual (25, s.Bar, "#4-3");
			// ... !!!
			Assert.AreEqual (2, s.PropertyValues.Count, "#4-3-2");
			Assert.AreEqual (0, s.Context.Count, "#4-4");

			// wrong provider
			props.Remove ("Bar");
			props.Add (new SettingsProperty ("Bar", typeof (int), provs ["MyProvider"], false, 10, SettingsSerializeAs.String, null, true, true));
			s = new MySettings ();
			s.Initialize (new SettingsContext (), props, provs);
			Assert.AreEqual (0, s.PropertyValues.Count, "#5-1");
			Assert.AreEqual (100, s.Foo, "#5-2");
			Assert.AreEqual (10, s.Bar, "#5-3");
		}

		[Test]
		public void AddPropertyTypeMismatch ()
		{
			SettingsPropertyCollection props = new SettingsPropertyCollection ();
			SettingsProviderCollection provs = new SettingsProviderCollection ();

			MyProvider p = new MyProvider ();
			MySettings s = new MySettings ();

			props.Add (new SettingsProperty ("Foo", typeof (string), p, false, 10, SettingsSerializeAs.String, null, true, true));
			provs.Add (p);

			s.Initialize (new SettingsContext (), props, provs);
			int i = s.Foo; // it still works as int, regardless of the settings property type...
		}

		[Test]
		[Ignore (".NET throws NRE, which means that it is not well designed.")]
		public void AddPropertyNoProviderButInProviders ()
		{
			SettingsPropertyCollection props = new SettingsPropertyCollection ();
			SettingsProviderCollection provs = new SettingsProviderCollection ();

			MyProvider p = new MyProvider ();
			MySettings s = new MySettings ();

			props.Add (new SettingsProperty ("Foo", typeof (string), null, false, 10, SettingsSerializeAs.String, null, true, true));
			provs.Add (p);

			s.Initialize (new SettingsContext (), props, provs);
			Assert.AreEqual (100, s.Foo);
		}

		[Test]
		public void ExceptionalGetPropertyValues ()
		{
			SettingsPropertyCollection props = new SettingsPropertyCollection ();
			SettingsProviderCollection provs = new SettingsProviderCollection ();

			MyProvider3 p = new MyProvider3 ();
			MySettings s = new MySettings ();

			props.Add (new SettingsProperty ("Foo", typeof (string), p, false, 10, SettingsSerializeAs.String, null, true, true));
			provs.Add (p);

			s.Initialize (new SettingsContext (), props, provs);
			Assert.AreEqual (0, s.Context.Count, "#0");
			try {
				Assert.AreEqual (100, s.Foo, "#1");
				Assert.Fail ("#2");
#if !TARGET_JVM
			} catch (Win32Exception) {
#else
			} catch (CustomerException) {
#endif
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProviderCollectionAddNameless ()
		{
			new SettingsProviderCollection ().Add (
				new MyProvider (true));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProviderCollectionAddDuplicate ()
		{
			SettingsProviderCollection c = new SettingsProviderCollection ();
			c.Add (new MyProvider ());
			c.Add (new MyProvider ());
		}

		class MyProvider3 : MyProvider
		{
			public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context, SettingsPropertyCollection props)
			{
#if !TARGET_JVM
				throw new Win32Exception (); // unlikely thrown otherwise.
#else
				throw new CustomerException (); // unlikely thrown otherwise.
#endif
			}
		}

		class MyProvider2 : MyProvider
		{
			public MyProvider2 (string item, object value)
				: base (item, value)
			{
			}

			public override string Name {
				get { return "MyProvider2"; }
			}
		}

		class MyProvider : SettingsProvider
		{
			bool bogus;
			string item;
			object default_value;

			public MyProvider ()
				: this (false)
			{
			}

			public MyProvider (bool bogus)
			{
				this.item = "Foo";
				default_value = 100;
				this.bogus = bogus;
			}

			public MyProvider (string item, object value)
			{
				this.item = item;
				this.default_value = value;
			}

			public override string Name {
				get { return bogus ? null : "MyProvider"; }
			}

			string app;
			public override string ApplicationName {
				get { return app; }
				set { app = value; }
			}

			public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context, SettingsPropertyCollection props)
			{
				SettingsPropertyValueCollection vals =
					new SettingsPropertyValueCollection ();
				foreach (SettingsProperty p in props)
					if (p.Provider == this) {
						SettingsPropertyValue pv = new SettingsPropertyValue (p);
						if (pv.Name == item)
							pv.PropertyValue = default_value;
						vals.Add (pv);
					}
				return vals;
			}

			public override void SetPropertyValues (SettingsContext context, SettingsPropertyValueCollection collection)
			{
				throw new Exception ();
			}
		}
	}
}

#endif
