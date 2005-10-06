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
// ToolBoxDataAttributeTest.cs
//
// Author:
//	Jackson Harper (jackson@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//


using System;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Web.UI {

	[TestFixture]
	public class ToolboxDataAttributeTest {

		[Test]
		public void Defaults ()
		{
			ToolboxDataAttribute at = new ToolboxDataAttribute (String.Empty);
			Assert.AreEqual (at.Data, String.Empty, "Data Value");

			Assert.AreEqual (ToolboxDataAttribute.Default.Data,
					String.Empty, "Default Data Value");
		}

		[Test]
		public void NullAllowed ()
		{
			ToolboxDataAttribute at = new ToolboxDataAttribute (null);
			Assert.AreEqual (at.Data, null, "Null Data");
		}
			
		[Test]
		public void EqualsTest ()
		{
			string foo_built = new StringBuilder ("f").Append ("oo").ToString ();
			ToolboxDataAttribute left = new ToolboxDataAttribute (foo_built);
			ToolboxDataAttribute right = new ToolboxDataAttribute ("foo");

			Assert.IsTrue (left.Equals (right), "Equals True");

			right = new ToolboxDataAttribute ("bar");
			Assert.IsFalse (left.Equals (right), "Equals False");

			Assert.IsFalse (left.Equals (45), "Equals Int");
			Assert.IsFalse (left.Equals ("foo"), "Equals String");
		}

		[Test]
		public void HashcodeTest ()
		{
			string foo_built = new StringBuilder ("f").Append ("oo").ToString ();
			ToolboxDataAttribute left = new ToolboxDataAttribute ("foo");
			ToolboxDataAttribute right = new ToolboxDataAttribute (foo_built);

			Assert.AreEqual (left.GetHashCode (), right.GetHashCode (), "Hash identity");

			left = new ToolboxDataAttribute (null);
			right = new ToolboxDataAttribute (null);

			Assert.AreEqual (left.GetHashCode (), right.GetHashCode (), "Hash identity (with null)");
		}
	}

}
