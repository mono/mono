//
// Tests for Microsoft.Web.ScriptEvent
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
	public class ScriptEventTest
	{
		class ActionPoker : Action {
			public override string TagName {
				get {
					return "poker";
				}
			}
		}

		[Test]
		public void Ctor ()
		{
			ActionPoker a = new ActionPoker();
			ScriptEvent e = new ScriptEvent (a, "HelloEvent", true);

			Assert.AreEqual ("HelloEvent", e.Name, "A1");
			Assert.IsTrue (e.SupportsActions, "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Null_Ctor1 ()
		{
			ScriptEvent e = new ScriptEvent (null, "HelloEvent", true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Null_Ctor2 ()
		{
			ActionPoker a = new ActionPoker();
			ScriptEvent e = new ScriptEvent (a, null, true);
		}

		[Test]
		public void Properties ()
		{
			ActionPoker a = new ActionPoker ();
			ScriptEvent e = new ScriptEvent (a, "HelloEvent", true);

			// defaults not specified in the ctor
			Assert.AreEqual ("", e.Handler, "A1");

			// getter/setter
			e.Handler = "foo";
			Assert.AreEqual ("foo", e.Handler, "A2");

			// setting to null
			e.Handler = null;
			Assert.AreEqual ("", e.Handler, "A5");
		}
		
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void No_SupportsActions ()
		{
			ActionPoker a = new ActionPoker ();
			ScriptEvent e = new ScriptEvent (a, "HelloEvent", false);

			Assert.IsNotNull (e.Actions, "A1");
			e.Actions.Add (new ActionPoker ());
		}

		[Test]
		public void SupportsActions ()
		{
			ActionPoker a = new ActionPoker ();
			ScriptEvent e = new ScriptEvent (a, "HelloEvent", true);

			Assert.IsNotNull (e.Actions, "A1");
			e.Actions.Add (new ActionPoker ());
			Assert.AreEqual (1, e.Actions.Count, "A2");
		}

		[Test]
		public void RenderActions ()
		{
			ActionPoker a = new ActionPoker ();
			ScriptEvent e = new ScriptEvent (a, "HelloEvent", true);

			StringWriter sw;
			ScriptTextWriter w;

			// test an empty event
			sw = new StringWriter();
			w = new ScriptTextWriter (sw);

			e.RenderActions (w);
			Assert.AreEqual ("", sw.ToString(), "A1");

			// now add an action and see what happens
			ActionPoker action = new ActionPoker ();
			action.ID = "action_id";
			action.Target = "action_target";

			e.Actions.Add (action);

			e.RenderActions (w);
			Assert.AreEqual ("<HelloEvent>\n  <poker id=\"action_id\" target=\"action_target\" />\n</HelloEvent>", sw.ToString().Replace ("\r\n", "\n"), "A2");
		}

		[Test]
		public void RenderHandlers ()
		{
			ActionPoker a = new ActionPoker ();
			ScriptEvent e = new ScriptEvent (a, "HelloEvent", true);

			StringWriter sw;
			ScriptTextWriter w;

			sw = new StringWriter();
			w = new ScriptTextWriter (sw);

			e.Handler = "hi there";

			e.RenderHandlers (w);
			Assert.AreEqual ("HelloEvent=\"hi there\"", sw.ToString(), "A1");
		}
	}
}
#endif
