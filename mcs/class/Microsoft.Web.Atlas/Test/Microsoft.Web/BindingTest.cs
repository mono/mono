//
// Tests for Microsoft.Web.Binding
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
	public class BindingTest
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

		[Test]
		public void Properties ()
		{
			Binding b = new Binding ();

			// default
			Assert.AreEqual (true, b.Automatic, "A1");
			Assert.AreEqual ("", b.DataContext, "A2");
			Assert.AreEqual ("", b.DataPath, "A3");
			Assert.AreEqual (BindingDirection.In, b.Direction, "A4");
			Assert.AreEqual ("", b.ID, "A5");
			Assert.AreEqual ("", b.Property, "A6");
			Assert.AreEqual ("", b.PropertyKey, "A7");
			Assert.IsNotNull (b.Transform, "A8");
			Assert.AreEqual ("", b.TransformerArgument, "A9");

			// getter/setter
			b.Automatic = false;
			Assert.AreEqual (false, b.Automatic, "A10");
			b.DataContext = "DataContext";
			Assert.AreEqual ("DataContext", b.DataContext, "A11");
			b.DataPath = "DataPath";
			Assert.AreEqual ("DataPath", b.DataPath, "A12");
			b.Direction = BindingDirection.InOut;
			Assert.AreEqual (BindingDirection.InOut, b.Direction, "A13");
			b.ID = "ID";
			Assert.AreEqual ("ID", b.ID, "A14");
			b.Property = "Property";
			Assert.AreEqual ("Property", b.Property, "A15");
			b.PropertyKey = "PropertyKey";
			Assert.AreEqual ("PropertyKey", b.PropertyKey, "A16");
			b.TransformerArgument = "TransformerArgument";
			Assert.AreEqual ("TransformerArgument", b.TransformerArgument, "A17");

			// setting to null
			b.DataContext = null;
			Assert.AreEqual ("", b.DataContext, "A18");
			b.DataPath = null;
			Assert.AreEqual ("", b.DataPath, "A19");
			b.ID = null;
			Assert.AreEqual ("", b.ID, "A20");
			b.Property = null;
			Assert.AreEqual ("", b.Property, "A21");
			b.PropertyKey = null;
			Assert.AreEqual ("", b.PropertyKey, "A22");
			b.TransformerArgument = null;
			Assert.AreEqual ("", b.TransformerArgument, "A23");
		}

		[Test]
		public void TransformEvent ()
		{
			Binding b = new Binding ();

			Assert.AreEqual ("", b.Transform.Handler, "A1");
			Assert.AreEqual (0, b.Transform.Actions.Count, "A2");
		}

		[Test]
		public void Render ()
		{
			Binding b = new Binding();
			StringWriter sw = new StringWriter();
			ScriptTextWriterPoker w = new ScriptTextWriterPoker (sw);

			b.Automatic = false;
			b.DataContext = "DataContext";
			b.DataPath = "DataPath";
			b.Direction = BindingDirection.InOut;
			b.ID = "ID";
			b.Property = "Property";
			b.PropertyKey = "PropertyKey";
			b.TransformerArgument = "TransformerArgument";

			b.RenderScript (w);

			Assert.AreEqual ("<binding automatic=\"False\" dataContext=\"DataContext\" dataPath=\"DataPath\" direction=\"InOut\" id=\"ID\" property=\"Property\" propertyKey=\"PropertyKey\" transformerArgument=\"TransformerArgument\" />", sw.ToString(), "A1");
		}

		void DoEvent (ScriptEventDescriptor e, string eventName, bool supportsActions)
		{
			Assert.AreEqual (eventName, e.EventName, eventName + " EventName");
			Assert.AreEqual (eventName, e.MemberName, eventName + " MemberName");
			Assert.AreEqual (supportsActions, e.SupportsActions, eventName + " SupportsActions");
		}

		void DoMethod (ScriptMethodDescriptor m, string methodName, string[] args)
		{
			Assert.AreEqual (methodName, m.MethodName, methodName + " MethodName");
			Assert.AreEqual (methodName, m.MemberName, methodName + " MemberName");
			Assert.AreEqual (args.Length, m.Parameters.Length, methodName + " Parameter count");
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
			Binding b = new Binding ();
			ScriptTypeDescriptor desc = ((IScriptObject)b).GetTypeDescriptor ();

			Assert.AreEqual (b, desc.ScriptObject, "A1");

			// events
			IEnumerable<ScriptEventDescriptor> events = desc.GetEvents();
			Assert.IsNotNull (events, "A2");

			IEnumerator<ScriptEventDescriptor> ee = events.GetEnumerator();
			Assert.IsTrue (ee.MoveNext(), "A3");
			DoEvent (ee.Current, "transform", false);
			Assert.IsFalse (ee.MoveNext(), "A4");

			// methods
			IEnumerable<ScriptMethodDescriptor> methods = desc.GetMethods();
			Assert.IsNotNull (methods, "A5");

			IEnumerator<ScriptMethodDescriptor> me = methods.GetEnumerator();
			Assert.IsTrue (me.MoveNext (), "A6");
			DoMethod (me.Current, "evaluateIn", new string[0]);
			Assert.IsTrue (me.MoveNext (), "A6");
			DoMethod (me.Current, "evaluateOut", new string[0]);
			Assert.IsFalse (me.MoveNext (), "A7");

			// properties
			IEnumerable<ScriptPropertyDescriptor> props = desc.GetProperties();
			Assert.IsNotNull (props, "A8");

			IEnumerator<ScriptPropertyDescriptor> pe = props.GetEnumerator();
			Assert.IsTrue (pe.MoveNext(), "A8");
			DoProperty (pe.Current, "automatic", ScriptType.Boolean, false, "Automatic");
			Assert.IsTrue (pe.MoveNext(), "A9");
			DoProperty (pe.Current, "dataContext", ScriptType.Object, false, "DataContext");
			Assert.IsTrue (pe.MoveNext(), "A7");
			DoProperty (pe.Current, "dataPath", ScriptType.String, false, "DataPath");
			Assert.IsTrue (pe.MoveNext(), "A8");
			DoProperty (pe.Current, "direction", ScriptType.Enum, false, "Direction");
			Assert.IsTrue (pe.MoveNext(), "A9");
			DoProperty (pe.Current, "id", ScriptType.String, false, "ID");
			Assert.IsTrue (pe.MoveNext(), "A10");
			DoProperty (pe.Current, "property", ScriptType.String, false, "Property");
			Assert.IsTrue (pe.MoveNext(), "A11");
			DoProperty (pe.Current, "propertyKey", ScriptType.String, false, "PropertyKey");
			Assert.IsTrue (pe.MoveNext(), "A12");
			DoProperty (pe.Current, "transformerArgument", ScriptType.String, false, "TransformerArgument");
			Assert.IsFalse (pe.MoveNext(), "A13");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsTypeDescriptorClosed ()
		{
			Binding b = new Binding ();
			ScriptTypeDescriptor desc = ((IScriptObject)b).GetTypeDescriptor ();

			desc.AddEvent (new ScriptEventDescriptor ("testEvent", true));
		}
	}
}
#endif
