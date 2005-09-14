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

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
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
		}

		[Test]
		public void Properties ()
		{
			ActionPoker a = new ActionPoker ();

			// default
			Assert.AreEqual ("", a.Target, "A1");

			// getter/setter
			a.Target = "foo";
			Assert.AreEqual ("foo", a.Target, "A2");

			// setting to null
			a.Target = null;
			Assert.AreEqual ("", a.Target, "A3");
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
	}
}
