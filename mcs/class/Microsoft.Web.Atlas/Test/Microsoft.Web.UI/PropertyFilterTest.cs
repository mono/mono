//
// Tests for Microsoft.Web.UI.PropertyFilter
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
	public class PropertyFilterTest
	{
		class Poker : PropertyFilter {
			public void AddAttributes (ScriptTextWriter w)
			{
				AddAttributesToElement (w);
			}
		}

		[Test]
		public void Properties ()
		{
			PropertyFilter f = new PropertyFilter ();

			// default
			Assert.AreEqual ("propertyFilter", f.TagName, "A1");
			Assert.AreEqual ("", f.Property, "A2");
			Assert.AreEqual ("", f.Value, "A3");

			// getter/setter
			f.Property = "Property";
			Assert.AreEqual ("Property", f.Property, "A4");
			f.Value = "Value";
			Assert.AreEqual ("Value", f.Value, "A5");

			// null setter
			f.Property = null;
			Assert.AreEqual ("", f.Property, "A6");
			f.Value = null;
			Assert.AreEqual ("", f.Value, "A7");
		}

		[Test]
		public void Render ()
		{
			PropertyFilter f = new PropertyFilter ();
			StringWriter sw;
			ScriptTextWriter w;

			sw = new StringWriter();
			w = new ScriptTextWriter (sw);

			f.Property = "Property";
			f.Value = "Value";

			((IScriptComponent)f).RenderScript (w);

			Assert.AreEqual ("<propertyFilter property=\"Property\" value=\"Value\" />", sw.ToString(), "A1");
		}

		void DoEvent (ScriptEventDescriptor e, string eventName, bool supportsActions)
		{
			Assert.AreEqual (eventName, e.EventName, eventName + " EventName");
			Assert.AreEqual (eventName, e.MemberName, eventName + " MemberName");
			Assert.AreEqual (supportsActions, e.SupportsActions, eventName + " SupportsActions");
		}

		void DoProperty (ScriptPropertyDescriptor p, string propertyName, ScriptType type, bool readOnly, string serverPropertyName)
		{
			Assert.AreEqual (propertyName, p.PropertyName, propertyName + " PropertyName");
			Assert.AreEqual (propertyName, p.MemberName, propertyName + " MemberName");
			Assert.AreEqual (serverPropertyName, p.ServerPropertyName, propertyName + " ServerPropertyName");
			Assert.AreEqual (readOnly, p.ReadOnly, propertyName + " ReadOnly");
			Assert.AreEqual (type, p.Type, propertyName + " Type");
		}

		[Test]
		public void TypeDescriptor ()
		{
			PropertyFilter f = new PropertyFilter();
			ScriptTypeDescriptor desc = ((IScriptObject)f).GetTypeDescriptor ();

			Assert.AreEqual (f, desc.ScriptObject, "A1");

			// events
			IEnumerable<ScriptEventDescriptor> events = desc.GetEvents();
			Assert.IsNotNull (events, "A2");

			IEnumerator<ScriptEventDescriptor> ee = events.GetEnumerator();
			Assert.IsTrue (ee.MoveNext(), "A3");
			DoEvent (ee.Current, "propertyChanged", true);
			Assert.IsFalse (ee.MoveNext(), "A4");

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
			Assert.IsTrue (pe.MoveNext(), "A11");
			DoProperty (pe.Current, "property", ScriptType.String, false, "Property");
			Assert.IsTrue (pe.MoveNext(), "A12");
			DoProperty (pe.Current, "value", ScriptType.String, false, "Value");
			Assert.IsFalse (pe.MoveNext(), "A13");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsTypeDescriptorClosed ()
		{
			HoverBehavior a = new HoverBehavior();
			ScriptTypeDescriptor desc = ((IScriptObject)a).GetTypeDescriptor ();

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
