//
// BindingElementTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MessagePropertiesTest
	{
		[Test]
		public void CopyProperties ()
		{
			var mp = new MessageProperties ();
			var obj = new object ();
			var obj2 = new object ();
			mp.Add ("FooProperty", obj);
			var mp2 = new MessageProperties ();
			mp2.Add ("BarProperty", obj2);
			mp.CopyProperties (mp2);
			Assert.AreEqual (obj, mp ["FooProperty"], "#1");
			Assert.AreEqual (obj2, mp ["BarProperty"], "#2");
		}

		[Test]
		public void AllowOutputBatching ()
		{
			var mp = new MessageProperties ();
			Assert.IsFalse (mp.AllowOutputBatching, "#0");
			mp.AllowOutputBatching = true;
			Assert.AreEqual (1, mp.Count, "#1");
			foreach (KeyValuePair<string,object> p in mp)
				Assert.AreEqual ("AllowOutputBatching", p.Key, "#2");
		}

		[Test]
		public void Encoder ()
		{
			var mp = new MessageProperties ();
			Assert.IsNull (mp.Via, "#0");
			mp.Encoder = new TextMessageEncodingBindingElement ().CreateMessageEncoderFactory ().Encoder;
			Assert.AreEqual (1, mp.Count, "#1");
			foreach (KeyValuePair<string,object> p in mp)
				Assert.AreEqual ("Encoder", p.Key, "#2");
		}

		[Test]
		public void Via ()
		{
			var mp = new MessageProperties ();
			Assert.IsNull (mp.Via, "#0");
			mp.Via = new Uri ("urn:foo");
			Assert.AreEqual (1, mp.Count, "#1");
			foreach (KeyValuePair<string,object> p in mp)
				Assert.AreEqual ("Via", p.Key, "#2");
		}
	}
}
