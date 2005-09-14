//
// Tests for Microsoft.Web.SetPropertyAction
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

namespace MonoTests.Microsoft.Web
{
	[TestFixture]
	public class SetPropertyActionTest
	{
		[Test]
		public void Properties ()
		{
			SetPropertyAction a = new SetPropertyAction ();

			// default
			Assert.AreEqual ("", a.Property, "A1");
			Assert.AreEqual ("", a.PropertyKey, "A2");
			Assert.AreEqual ("", a.Value, "A3");
			Assert.AreEqual ("setProperty", a.TagName, "A4");

			// getter/setter
			a.Property = "property";
			Assert.AreEqual ("property", a.Property, "A5");

			a.PropertyKey = "propertykey";
			Assert.AreEqual ("propertykey", a.PropertyKey, "A6");

			a.Value = "value";
			Assert.AreEqual ("value", a.Value, "A7");

			// setting to null
			a.Property = null;
			Assert.AreEqual ("", a.Property, "A8");
			a.PropertyKey = null;
			Assert.AreEqual ("", a.PropertyKey, "A9");
			a.Value = null;
			Assert.AreEqual ("", a.Value, "A10");
		}

		[Test]
		public void Render ()
		{
			SetPropertyAction a = new SetPropertyAction ();
			StringWriter sw;
			ScriptTextWriter w;

			// test an empty action
			sw = new StringWriter();
			w = new ScriptTextWriter (sw);
			a.RenderAction (w);

			Assert.AreEqual ("<setProperty />", sw.ToString(), "A1");

			// test with a property
			a.Target = "target";
			a.Property = "property";
			a.PropertyKey = "propertyKey";
			a.Value = "value";

			sw = new StringWriter();
			w = new ScriptTextWriter (sw);
			a.RenderAction (w);

			Assert.AreEqual ("<setProperty target=\"target\" property=\"property\" propertyKey=\"propertyKey\" value=\"value\" />", sw.ToString(), "A2");

			// test with a target and id
			a.ID = "set_id";
			a.Target = "target";
			a.Property = "property";
			a.PropertyKey = "propertyKey";
			a.Value = "value";

			sw = new StringWriter();
			w = new ScriptTextWriter (sw);
			a.RenderAction (w);

			Assert.AreEqual ("<setProperty id=\"set_id\" target=\"target\" property=\"property\" propertyKey=\"propertyKey\" value=\"value\" />", sw.ToString(), "A3");
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
			Assert.AreEqual (readOnly, p.ReadOnly, propertyName + " ReadOnly");
			Assert.AreEqual (type, p.Type, propertyName + " Type");
		}

		[Test]
		public void TypeDescriptor ()
		{
			SetPropertyAction a = new SetPropertyAction();
			ScriptTypeDescriptor desc = ((IScriptObject)a).GetTypeDescriptor ();

			Assert.AreEqual (a, desc.ScriptObject, "A1");

			// events
			IEnumerable<ScriptEventDescriptor> events = desc.GetEvents();
			Assert.IsNotNull (events, "A2");

			IEnumerator<ScriptEventDescriptor> ee = events.GetEnumerator();
			Assert.IsTrue (ee.MoveNext());
			DoEvent (ee.Current, "propertyChanged", true);
			Assert.IsFalse (ee.MoveNext());

			// methods
			IEnumerable<ScriptMethodDescriptor> methods = desc.GetMethods();
			Assert.IsNotNull (methods, "A3");

			IEnumerator<ScriptMethodDescriptor> me = methods.GetEnumerator();
			Assert.IsFalse (me.MoveNext ());

			// properties
			IEnumerable<ScriptPropertyDescriptor> props = desc.GetProperties();
			Assert.IsNotNull (props, "A4");

			IEnumerator<ScriptPropertyDescriptor> pe = props.GetEnumerator();
			Assert.IsTrue (pe.MoveNext(), "A5");
			DoProperty (pe.Current, "bindings", ScriptType.Array, true, "Bindings");
			Assert.IsTrue (pe.MoveNext(), "A6");
			DoProperty (pe.Current, "dataContext", ScriptType.Object, false, "");
			Assert.IsTrue (pe.MoveNext(), "A7");
			DoProperty (pe.Current, "id", ScriptType.String, false, "ID");
			Assert.IsTrue (pe.MoveNext(), "A8");
			DoProperty (pe.Current, "eventArgs", ScriptType.Object, false, "");
			Assert.IsTrue (pe.MoveNext(), "A9");
			DoProperty (pe.Current, "result", ScriptType.Object, false, "");
			Assert.IsTrue (pe.MoveNext(), "A10");
			DoProperty (pe.Current, "sender", ScriptType.Object, false, "");
			Assert.IsTrue (pe.MoveNext(), "A11");
			DoProperty (pe.Current, "sequence", ScriptType.Enum, false, "Sequence");
			Assert.IsTrue (pe.MoveNext(), "A12");
			DoProperty (pe.Current, "target", ScriptType.Object, false, "Target");
			Assert.IsTrue (pe.MoveNext(), "A13");
			DoProperty (pe.Current, "property", ScriptType.String, false, "Property");
			Assert.IsTrue (pe.MoveNext(), "A14");
			DoProperty (pe.Current, "propertyKey", ScriptType.String, false, "PropertyKey");
			Assert.IsTrue (pe.MoveNext(), "A15");
			DoProperty (pe.Current, "value", ScriptType.String, false, "Value");
			Assert.IsFalse (pe.MoveNext(), "A16");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsTypeDescriptorClosed ()
		{
			SetPropertyAction a = new SetPropertyAction();
			ScriptTypeDescriptor desc = ((IScriptObject)a).GetTypeDescriptor ();

			desc.AddEvent (new ScriptEventDescriptor ("testEvent", true));
		}
	}
}
#endif
