//
// SettingElementTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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

#if NET_2_0

using System;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Configuration
{
	[TestFixture]
	public class SettingElementTest
	{
		[Test]
		public void Initial ()
		{
			SettingElement el = new SettingElement ();
			Assert.IsNotNull (el.Value, "#1");
			Assert.IsNull (el.Value.ValueXml, "#2");
		}

		[Test]
		public void CollectionAddNull ()
		{
			try {
				SettingElementCollection c = new SettingElementCollection ();
				c.Add (null);
				Assert.Fail ();
			} catch (NullReferenceException) {
				// .net s cks here
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void CollectionAddNameless ()
		{
			SettingElement el = new SettingElement ();
			Assert.AreEqual (String.Empty, el.Name, "premise #1");
			SettingElementCollection c = new SettingElementCollection ();
			Assert.AreEqual (ConfigurationElementCollectionType.BasicMap, c.CollectionType, "premise #2");
			c.Add (el);
			Assert.AreEqual (el, c.Get (""), "#1");
		}

		[Test]
		public void CollectionGetNonExistent ()
		{
			SettingElementCollection c = new SettingElementCollection ();
			Assert.IsNull (c.Get ("nonexistent"));
		}
	}
}

#endif
