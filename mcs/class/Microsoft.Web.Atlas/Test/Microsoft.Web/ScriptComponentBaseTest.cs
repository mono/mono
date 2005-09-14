//
// Tests for Microsoft.Web.ScriptComponentBase
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
using System.Collections;
using System.IO;
using Microsoft.Web;

namespace MonoTests.Microsoft.Web
{
	[TestFixture]
	public class ScriptComponentBaseTest
	{
		class ScriptComponentBasePoker : ScriptComponentBase {
			
			public IScriptObject GetOwner () {
				return base.Owner;
			}

			public ScriptEvent GetPropertyChanged () {
				return base.PropertyChanged;
			}

			public ScriptEventCollection GetScriptEvents () {
				return base.ScriptEvents;
			}

			public override string TagName {
				get {
					return "poker";
				}
			}
		}

		[Test]
		public void Properties ()
		{
			ScriptComponentBasePoker poker = new ScriptComponentBasePoker ();

			Assert.AreEqual ("", poker.ID, "A1");
			Assert.IsNull (poker.GetOwner(), "A2");

			BindingCollection bindings = poker.Bindings;
			Assert.IsNotNull (bindings, "A3");
			DoBindings (poker, bindings);

			Assert.IsNotNull (poker.GetPropertyChanged(), "A7");
			DoPropertyChanged (poker, poker.GetPropertyChanged());

			Assert.IsNotNull (poker.GetScriptEvents(), "A8");
			DoScriptEvents (poker, poker.GetScriptEvents());
		}

		void DoBindings (IScriptObject owner, BindingCollection bindings)
		{
			Assert.AreEqual (owner, ((IScriptObject)bindings).Owner, "A4");
			Assert.AreEqual (0, bindings.Count, "b1");
			Assert.AreEqual ("", ((IScriptObject)bindings).ID, "b2");
		}

		void DoPropertyChanged (IScriptObject owner, ScriptEvent PropertyChanged)
		{
			Assert.AreEqual ("propertyChanged", PropertyChanged.Name, "p1");
			Assert.AreEqual (true, PropertyChanged.SupportsActions, "p2");
			Assert.AreEqual ("", PropertyChanged.Handler, "p3");
			Assert.IsNotNull (PropertyChanged.Actions, "p4");

			DoActions (owner, PropertyChanged.Actions);
		}

		void DoActions (IScriptObject owner, ActionCollection Actions)
		{
			Assert.AreEqual (owner, ((IScriptObject)Actions).Owner, "a1");
			Assert.AreEqual (0, Actions.Count, "a2");
		}

		void DoScriptEvents (IScriptObject owner, ScriptEventCollection ScriptEvents)
		{
			Assert.AreEqual (1, ((ICollection)ScriptEvents).Count, "e1");
			IEnumerator<ScriptEvent> e = ((IEnumerable<ScriptEvent>)ScriptEvents).GetEnumerator();

			e.MoveNext();

			ScriptEvent ev = e.Current;

			Assert.AreEqual ("propertyChanged", ev.Name, "p1");
			Assert.AreEqual (true, ev.SupportsActions, "p2");
			Assert.AreEqual ("", ev.Handler, "p3");
			Assert.IsNotNull (ev.Actions, "p4");

			DoActions (owner, ev.Actions);
		}
	}
}
#endif
