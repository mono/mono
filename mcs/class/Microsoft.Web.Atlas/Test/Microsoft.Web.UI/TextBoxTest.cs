//
// Tests for Microsoft.Web.UI.TextBox
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
	public class TextBoxTest : ScriptControlTest
	{
		[Test]
		public void Properties ()
		{
			TextBox t = new TextBox ();

			// defaults
			Assert.AreEqual ("textBox", t.TagName, "A1");
			Assert.AreEqual (1000, t.AutoCompletionInterval, "A2");
			Assert.AreEqual (3, t.AutoCompletionMinimumPrefixLength, "A3");
			Assert.AreEqual (null, t.AutoCompletionServiceMethod, "A4");
			Assert.AreEqual (null, t.AutoCompletionServiceUrl, "A5");
			Assert.AreEqual (10, t.AutoCompletionSetCount, "A6");
			Assert.AreEqual (0, t.Size, "A7");
			Assert.AreEqual ("", t.Text, "A8");

			// get/set
			t.AutoCompletionInterval = 5;
			Assert.AreEqual (5, t.AutoCompletionInterval, "A9");
			t.AutoCompletionMinimumPrefixLength = 50;
			Assert.AreEqual (50, t.AutoCompletionMinimumPrefixLength, "A10");
			t.AutoCompletionServiceMethod = "foo";
			Assert.AreEqual ("foo", t.AutoCompletionServiceMethod, "A11");
			t.AutoCompletionServiceUrl = "bar";
			Assert.AreEqual ("bar", t.AutoCompletionServiceUrl, "A12");
			t.AutoCompletionSetCount = 25;
			Assert.AreEqual (25, t.AutoCompletionSetCount, "A13");
			t.Size = 50;
			Assert.AreEqual (50, t.Size, "A14");
			t.Text = "hi";
			Assert.AreEqual ("hi", t.Text, "A15");

			// null set
			t.AutoCompletionServiceMethod = null;
			Assert.AreEqual (null, t.AutoCompletionServiceMethod, "A16");
			t.AutoCompletionServiceUrl = null;
			Assert.AreEqual (null, t.AutoCompletionServiceUrl, "A17");
			t.Text = null;
			Assert.AreEqual ("", t.Text, "A18");
		}

		[Test]
		public void TypeDescriptor ()
		{
			TextBox t = new TextBox();

			ScriptTypeDescriptor desc = ((IScriptObject)t).GetTypeDescriptor ();

			Assert.AreEqual (t, desc.ScriptObject, "A1");

			// events
			IEnumerable<ScriptEventDescriptor> events = desc.GetEvents();
			Assert.IsNotNull (events, "A2");

			IEnumerator<ScriptEventDescriptor> ee = events.GetEnumerator();
			Assert.IsTrue (ee.MoveNext(), "A3");
			DoEvent (ee.Current, "propertyChanged", true);
			Assert.IsFalse (ee.MoveNext(), "A4");

			// methods
			string[] args;
			IEnumerable<ScriptMethodDescriptor> methods = desc.GetMethods();
			Assert.IsNotNull (methods, "A5");

			IEnumerator<ScriptMethodDescriptor> me = methods.GetEnumerator();
			Assert.IsTrue (me.MoveNext(), "A6");
			args = new string[1];
			args[0] = "className";
			DoMethod (me.Current, "addCssClass", args);
			Assert.IsTrue (me.MoveNext(), "A7");
			DoMethod (me.Current, "focus", new string[0]);
			Assert.IsTrue (me.MoveNext(), "A8");
			DoMethod (me.Current, "scrollIntoView", new string[0]);
			Assert.IsTrue (me.MoveNext(), "A9");
			args = new string[1];
			args[0] = "className";
			DoMethod (me.Current, "removeCssClass", args);
			Assert.IsTrue (me.MoveNext(), "A10");
			args = new string[1];
			args[0] = "className";
			DoMethod (me.Current, "toggleCssClass", args);
			Assert.IsFalse (me.MoveNext (), "A10");

			// properties
			IEnumerable<ScriptPropertyDescriptor> props = desc.GetProperties();
			Assert.IsNotNull (props, "A11");

			IEnumerator<ScriptPropertyDescriptor> pe = props.GetEnumerator();
			Assert.IsTrue (pe.MoveNext(), "A12");
			DoProperty (pe.Current, "bindings", ScriptType.Array, true, "Bindings");
			Assert.IsTrue (pe.MoveNext(), "A13");
			DoProperty (pe.Current, "dataContext", ScriptType.Object, false, "");
			Assert.IsTrue (pe.MoveNext(), "A14");
			DoProperty (pe.Current, "id", ScriptType.String, false, "ID");
			Assert.IsTrue (pe.MoveNext(), "A15");
			DoProperty (pe.Current, "associatedElement", ScriptType.Object, true, "");
			Assert.IsTrue (pe.MoveNext(), "A16");
			DoProperty (pe.Current, "behaviors", ScriptType.Array, true, "Behaviors");
			Assert.IsTrue (pe.MoveNext(), "A17");
			DoProperty (pe.Current, "cssClass", ScriptType.String, false, "CssClass");
			Assert.IsTrue (pe.MoveNext(), "A18");
			DoProperty (pe.Current, "enabled", ScriptType.Boolean, false, "Enabled");
			Assert.IsTrue (pe.MoveNext(), "A19");
			DoProperty (pe.Current, "style", ScriptType.Object, true, "");
			Assert.IsTrue (pe.MoveNext(), "A20");
			DoProperty (pe.Current, "visible", ScriptType.Boolean, false, "Visible");
			Assert.IsTrue (pe.MoveNext(), "A21");
			DoProperty (pe.Current, "visibilityMode", ScriptType.Enum, false, "VisibilityMode");
			Assert.IsTrue (pe.MoveNext(), "A21");
			DoProperty (pe.Current, "validators", ScriptType.Array, true, "Validators");
			Assert.IsTrue (pe.MoveNext(), "A22");
			DoProperty (pe.Current, "text", ScriptType.String, false, "Text");
			Assert.IsFalse (pe.MoveNext(), "A23");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsTypeDescriptorClosed ()
		{
			TextBox t = new TextBox ();
			ScriptTypeDescriptor desc = ((IScriptObject)t).GetTypeDescriptor ();

			desc.AddEvent (new ScriptEventDescriptor ("testEvent", true));
		}
	}
}
#endif
