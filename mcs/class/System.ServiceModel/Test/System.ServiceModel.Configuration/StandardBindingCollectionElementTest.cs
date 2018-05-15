//
// StandardBindingCollectionElementTest.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.ServiceModel.Configuration;
using System.ServiceModel;
using System.Configuration;
using System.Collections.ObjectModel;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class StandardBindingCollectionElementTest
	{
		class Poker : StandardBindingCollectionElement<BasicHttpBinding, BasicHttpBindingElement>
		{
			public ConfigurationPropertyCollection GetProperties () {
				return Properties;
			}

			[ConfigurationProperty ("myProperty")]
			string MyProperty {
				get { return "myProperty"; }
				set { }
			}
		}

		[Test]
		public void Properties () {
			Poker poker = new Poker ();
			ConfigurationPropertyCollection coll = poker.GetProperties ();
			Assert.AreEqual (1, coll.Count, "Count");

			foreach (ConfigurationProperty prop in coll) {
				Assert.AreEqual ("", prop.Name);
			}
		}

		[Test]
		public void Properties2 () {

			Poker p1 = new Poker ();
			Poker p2 = new Poker ();

			Assert.AreEqual (false, p1.GetProperties ().Contains ("myProperty"), "Contains myProperty");
			Assert.AreEqual (false, p1.GetProperties () == p2.GetProperties (), "#");
		}

		[Test]
		public void ConfiguredBindings () {
			Poker poker = new Poker ();
			Assert.AreEqual (0, poker.ConfiguredBindings.Count, "Count #1");

			BasicHttpBindingElement elem = new BasicHttpBindingElement ("my_binding");
			poker.Bindings.Add (elem);
			Assert.AreEqual (1, poker.ConfiguredBindings.Count, "Count #2");

			Assert.AreEqual (elem, poker.ConfiguredBindings [0], "Instance");
		}
	}
}
#endif

