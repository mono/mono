//
// Tests for Microsoft.Web.UI.ClickBehavior
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//

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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Web;
using Microsoft.Web.UI;

namespace MonoTests.Microsoft.Web.UI
{
	[TestFixture]
	public class ClickBehaviorTest : ScriptControlTest
	{
		[Test]
		public void Properties ()
		{
			ClickBehavior b = new ClickBehavior ();

			// default
			Assert.AreEqual ("clickBehavior", b.TagName, "A1");
		}

		[Test]
		public void Render ()
		{
			ClickBehavior c = new ClickBehavior ();
			StringWriter sw;
			ScriptTextWriter w;

			sw = new StringWriter();
			w = new ScriptTextWriter (sw);
			((IScriptComponent)c).RenderScript (w);

			Assert.AreEqual ("", sw.ToString(), "A1");
		}

		[Test]
		public void TypeDescriptor ()
		{
			ClickBehavior a = new ClickBehavior();
			ScriptTypeDescriptor desc = ((IScriptObject)a).GetTypeDescriptor ();

			Assert.AreEqual (a, desc.ScriptObject, "A1");

			// events
			IEnumerable<ScriptEventDescriptor> events = desc.GetEvents();
			Assert.IsNotNull (events, "A2");

			IEnumerator<ScriptEventDescriptor> ee = events.GetEnumerator();
			Assert.IsTrue (ee.MoveNext(), "A3");
			DoEvent (ee.Current, "propertyChanged", true);
			Assert.IsTrue (ee.MoveNext(), "A4");
			DoEvent (ee.Current, "click", true);
			Assert.IsFalse (ee.MoveNext(), "A5");

			// methods
			IEnumerable<ScriptMethodDescriptor> methods = desc.GetMethods();
			Assert.IsNotNull (methods, "A6");

			IEnumerator<ScriptMethodDescriptor> me = methods.GetEnumerator();
			Assert.IsFalse (me.MoveNext ());

			// properties
			IEnumerable<ScriptPropertyDescriptor> props = desc.GetProperties();
			Assert.IsNotNull (props, "A7");

			IEnumerator<ScriptPropertyDescriptor> pe = props.GetEnumerator();
			Assert.IsTrue (pe.MoveNext(), "A8");
			DoProperty (pe.Current, "bindings", ScriptType.Array, true, "Bindings");
			Assert.IsTrue (pe.MoveNext(), "A9");
			DoProperty (pe.Current, "dataContext", ScriptType.Object, false, "");
			Assert.IsTrue (pe.MoveNext(), "A10");
			DoProperty (pe.Current, "id", ScriptType.String, false, "ID");
			Assert.IsFalse (pe.MoveNext(), "A11");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsTypeDescriptorClosed ()
		{
			ClickBehavior a = new ClickBehavior();
			ScriptTypeDescriptor desc = ((IScriptObject)a).GetTypeDescriptor ();

			desc.AddEvent (new ScriptEventDescriptor ("testEvent", true));
		}
	}
}
#endif
