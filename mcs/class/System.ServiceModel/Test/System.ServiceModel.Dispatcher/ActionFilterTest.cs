//
// ActionMessageFilterTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using NUnit.Framework;

using Element = System.ServiceModel.Channels.TextMessageEncodingBindingElement;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class ActionMessageFilterTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull ()
		{
			new ActionMessageFilter (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull2 ()
		{
			new ActionMessageFilter ("foo", null);
		}

		[Test]
		public void Match ()
		{
			ActionMessageFilter f = new ActionMessageFilter ("foo");
			Assert.AreEqual (1, f.Actions.Count, "#1");
			Message msg = Message.CreateMessage (MessageVersion.Default, "foo");
			Assert.AreEqual ("foo", msg.Headers.Action, "#2");
			Assert.IsTrue (f.Match (msg), "#3");
			msg = Message.CreateMessage (MessageVersion.Default, "bar");
			Assert.IsFalse (f.Match (msg), "#4");

			f = new ActionMessageFilter ("foo", "bar");
			Assert.AreEqual (2, f.Actions.Count, "#5");
			Assert.IsTrue (f.Match (msg), "#6");
		}

/*
		[Test]
		public void CreateMessageFilterTable ()
		{
			ActionMessageFilter f = new ActionMessageFilter ("foo");
			IMessageFilterTable<int> t = f.CreateFilterTable<int> ();
			Assert.AreEqual (0, t.Count, "#1");

			t.Add (f, 0);
			Assert.AreEqual (1, t.Count, "#2");
		}
*/
	}
}
#endif