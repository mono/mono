//
// Tests for Microsoft.Web.Action
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
	public class ActionTest
	{
		class ScriptTextWriterPoker : ScriptTextWriter {
			public ScriptTextWriterPoker (TextWriter w) : base (w) {}

#if SPEW
			public override void WriteStartElement (string prefix, string localName, string ns) {
				Console.WriteLine ("WriteStartElement ({0}, {1}, {2})", prefix, localName, ns);
				Console.WriteLine (Environment.StackTrace);
				base.WriteStartElement (prefix, localName, ns);
			}

			public override void WriteEndElement () {
				Console.WriteLine ("WriteEndElement");
				Console.WriteLine (Environment.StackTrace);
				base.WriteEndElement ();
			}

			public override void WriteString (string s) {
				Console.WriteLine ("WriteString {0}", s);
				base.WriteString (s);
			}

			public override void WriteName (string name) {
				Console.WriteLine ("WriteName {0}", name);
				base.WriteName (name);
			}

			public override void WriteWhitespace (string ws) {
				Console.WriteLine ("WriteWhitespace");
				base.WriteWhitespace (ws);
			}

			public override void WriteRaw (string data) {
				Console.WriteLine ("ScriptTextWriter.WriteRaw ({0}) {1}", data, Environment.StackTrace);
				base.WriteRaw (data);
			}
#endif
		}

		class ActionPoker : Action {
			public StringWriter Writer;

#if SPEW
			protected override void AddAttributesToElement (ScriptTextWriter writer) {
				Console.WriteLine ("'" + Writer.ToString() + "'");
				Console.WriteLine (Environment.StackTrace);
				base.AddAttributesToElement (writer);
			}

			protected override void RenderScriptBeginTag (ScriptTextWriter writer) {
				Console.WriteLine ("'" + Writer.ToString() + "'");
				Console.WriteLine (Environment.StackTrace);
				base.RenderScriptBeginTag (writer);
			}

			protected override void RenderScriptEndTag (ScriptTextWriter writer) {
				Console.WriteLine ("'" + Writer.ToString() + "'");
				Console.WriteLine (Environment.StackTrace);
				base.RenderScriptEndTag (writer);
			}

			protected override void RenderScriptTagContents (ScriptTextWriter writer) {
				Console.WriteLine ("'" + Writer.ToString() + "'");
				Console.WriteLine (Environment.StackTrace);
				base.RenderScriptTagContents (writer);
			}
#endif
			public override string TagName {
				get {
					return "poker";
				}
			}

			public void InitTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
			{
				base.InitializeTypeDescriptor (typeDescriptor);
			}
		}

		[Test]
		public void Properties ()
		{
			ActionPoker a = new ActionPoker ();

			// default
			Assert.AreEqual ("", a.Target, "A1");
			Assert.AreEqual (ActionSequence.AfterEventHandler, a.Sequence, "A2");

			// getter/setter
			a.Target = "foo";
			Assert.AreEqual ("foo", a.Target, "A3");

			a.Sequence = ActionSequence.BeforeEventHandler;
			Assert.AreEqual (ActionSequence.BeforeEventHandler, a.Sequence, "A4");

			// setting to null
			a.Target = null;
			Assert.AreEqual ("", a.Target, "A5");
		}

		[Test]
		public void Render ()
		{
			ActionPoker a = new ActionPoker ();
			StringWriter sw;
			ScriptTextWriter w;

			// test an empty action
			sw = new StringWriter();
			w = new ScriptTextWriterPoker (sw);
			a.Writer = sw;
			a.RenderAction (w);

			Assert.AreEqual ("<poker />", sw.ToString(), "A1");

			// test with a target
			a.Target = "foo";

			sw = new StringWriter();
			w = new ScriptTextWriterPoker (sw);
			a.Writer = sw;
			a.RenderAction (w);

			Assert.AreEqual ("<poker target=\"foo\" />", sw.ToString(), "A2");

			// test with a target and id
			a.ID = "poker_action";
			a.Target = "foo";

			sw = new StringWriter();
			w = new ScriptTextWriterPoker (sw);
			a.Writer = sw;
			a.RenderAction (w);

			Assert.AreEqual ("<poker id=\"poker_action\" target=\"foo\" />", sw.ToString(), "A3");
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
			ActionPoker a = new ActionPoker ();
			ScriptTypeDescriptor desc = new ScriptTypeDescriptor(a);

			a.InitTypeDescriptor (desc);

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
			Assert.IsFalse (pe.MoveNext(), "A13");
		}

		[Test]
		//		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeDescriptorClosed ()
		{
			/* looks like the TypeDescriptor isn't closed after the call to InitializeTypeDescriptor... go figure */
			ActionPoker a = new ActionPoker ();
			ScriptTypeDescriptor desc = new ScriptTypeDescriptor(a);

			a.InitTypeDescriptor (desc);

			desc.AddEvent (new ScriptEventDescriptor ("testEvent", true));
		}
	}
}
#endif
