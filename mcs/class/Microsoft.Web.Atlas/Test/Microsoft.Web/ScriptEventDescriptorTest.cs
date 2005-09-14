//
// Tests for Microsoft.Web.ScriptEventDescriptor
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
	public class ScriptEventDescriptorTest
	{
		[Test]
		public void ctor1 ()
		{
			ScriptEventDescriptor sd = new ScriptEventDescriptor ("eventName", true);
			Assert.AreEqual ("eventName", sd.EventName, "A1");
			Assert.AreEqual ("eventName", sd.MemberName, "A2");
			Assert.AreEqual (true, sd.SupportsActions, "A3");
			Assert.AreEqual ("", sd.ServerPropertyName, "A4");
		}

		[Test]
		public void ctor2 ()
		{
			ScriptEventDescriptor sd = new ScriptEventDescriptor ("eventName", true, "serverPropertyName");
			Assert.AreEqual ("eventName", sd.EventName, "A1");
			Assert.AreEqual ("eventName", sd.MemberName, "A2");
			Assert.AreEqual (true, sd.SupportsActions, "A3");
			Assert.AreEqual ("serverPropertyName", sd.ServerPropertyName, "A4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void nulltest1 ()
		{
			ScriptEventDescriptor sd = new ScriptEventDescriptor (null, true);
		}

		[Test]
		public void nulltest2 ()
		{
			ScriptEventDescriptor sd = new ScriptEventDescriptor ("eventName", true, null);

			Assert.AreEqual ("", sd.ServerPropertyName, "A1");
		}
	}
}
#endif
