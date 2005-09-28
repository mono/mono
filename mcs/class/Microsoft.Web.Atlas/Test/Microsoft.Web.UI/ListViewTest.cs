//
// Tests for Microsoft.Web.UI.ListView
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
	public class ListViewTest : ScriptControlTest
	{
		class Poker : ListView {
			public void AddAttributes (ScriptTextWriter w)
			{
				AddAttributesToElement (w);
			}
		}

		[Test]
		public void Properties ()
		{
			ListView l = new ListView ();

			// defaults
			Assert.AreEqual ("listView", l.TagName, "A1");
			Assert.AreEqual ("", l.AlternatingItemCssClass, "A2");
			Assert.AreEqual ("", l.ItemCssClass, "A3");
			Assert.AreEqual ("", l.ItemTemplateControlID, "A3");
			Assert.AreEqual ("", l.SeparatorTemplateControlID, "A3");

			// get/set
			l.AlternatingItemCssClass = "AlternatingItemCssClass";
			Assert.AreEqual ("AlternatingItemCssClass", l.AlternatingItemCssClass, "A4");
			l.ItemCssClass = "ItemCssClass";
			Assert.AreEqual ("ItemCssClass", l.ItemCssClass, "A5");
			l.ItemTemplateControlID = "ItemTemplateControlID";
			Assert.AreEqual ("ItemTemplateControlID", l.ItemTemplateControlID, "A6");
			l.SeparatorTemplateControlID = "SeparatorTemplateControlID";
			Assert.AreEqual ("SeparatorTemplateControlID", l.SeparatorTemplateControlID, "A7");

			// null set
			l.AlternatingItemCssClass = null;
			Assert.AreEqual ("", l.AlternatingItemCssClass, "A4");
			l.ItemCssClass = null;
			Assert.AreEqual ("", l.ItemCssClass, "A5");
			l.ItemTemplateControlID = null;
			Assert.AreEqual ("", l.ItemTemplateControlID, "A6");
			l.SeparatorTemplateControlID = null;
			Assert.AreEqual ("", l.SeparatorTemplateControlID, "A7");
		}

		[Test]
		public void TypeDescriptor ()
		{
			ListView l = new ListView();
			ScriptTypeDescriptor desc = ((IScriptObject)l).GetTypeDescriptor ();

			Assert.AreEqual (l, desc.ScriptObject, "A1");

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
			DoProperty (pe.Current, "alternatingItemCssClass", ScriptType.String, false, "AlternatingItemCssClass");
			Assert.IsTrue (pe.MoveNext(), "A22");
			DoProperty (pe.Current, "data", ScriptType.Object, false, "");
			Assert.IsTrue (pe.MoveNext(), "A23");
			DoProperty (pe.Current, "length", ScriptType.Number, true, "");
			Assert.IsTrue (pe.MoveNext(), "A24");
			DoProperty (pe.Current, "layoutTemplate", ScriptType.Object, false, "");
			Assert.IsTrue (pe.MoveNext(), "A25");
			DoProperty (pe.Current, "itemCssClass", ScriptType.String, false, "ItemCssClass");
			Assert.IsTrue (pe.MoveNext(), "A26");
			DoProperty (pe.Current, "itemTemplateParentElementId", ScriptType.String, false, "");
			Assert.IsTrue (pe.MoveNext(), "A27");
			DoProperty (pe.Current, "separatorTemplate", ScriptType.Object, false, "");
			Assert.IsTrue (pe.MoveNext(), "A28");
			DoProperty (pe.Current, "emptyTemplate", ScriptType.Object, false, "");
			Assert.IsFalse (pe.MoveNext(), "A30");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsTypeDescriptorClosed ()
		{
			ListView l = new ListView ();
			ScriptTypeDescriptor desc = ((IScriptObject)l).GetTypeDescriptor ();

			desc.AddEvent (new ScriptEventDescriptor ("testEvent", true));
		}

		[Test]
		public void Attributes ()
		{
			Poker c = new Poker ();
			StringWriter sw;
			ScriptTextWriter w;

			sw = new StringWriter();
			w = new ScriptTextWriter (sw);

			c.AddAttributes (w);

			Assert.AreEqual ("", sw.ToString(), "A1");
		}
	}
}
#endif
