//
// Tests for Microsoft.Web.ScriptPropertyDescriptor
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
	public class ScriptPropertyDescriptorTest
	{
		[Test]
		public void ctor1 ()
		{
			ScriptPropertyDescriptor sd = new ScriptPropertyDescriptor ("propertyName", ScriptType.Object);
			Assert.AreEqual ("propertyName", sd.PropertyName, "A1");
			Assert.AreEqual ("propertyName", sd.MemberName, "A2");
			Assert.AreEqual (ScriptType.Object, sd.Type, "A3");
			Assert.IsFalse (sd.ReadOnly, "A4");
			Assert.AreEqual ("", sd.ServerPropertyName, "A5");
		}

		[Test]
		public void ctor2 ()
		{
			ScriptPropertyDescriptor sd = new ScriptPropertyDescriptor ("propertyName", ScriptType.Object, "serverPropertyName");
			Assert.AreEqual ("propertyName", sd.PropertyName, "A1");
			Assert.AreEqual ("propertyName", sd.MemberName, "A2");
			Assert.AreEqual (ScriptType.Object, sd.Type, "A3");
			Assert.IsFalse (sd.ReadOnly, "A4");
			Assert.AreEqual ("serverPropertyName", sd.ServerPropertyName, "A5");
		}

		[Test]
		public void ctor3 ()
		{
			ScriptPropertyDescriptor sd = new ScriptPropertyDescriptor ("propertyName", ScriptType.Object, true);
			Assert.AreEqual ("propertyName", sd.PropertyName, "A1");
			Assert.AreEqual ("propertyName", sd.MemberName, "A2");
			Assert.AreEqual (ScriptType.Object, sd.Type, "A3");
			Assert.IsTrue (sd.ReadOnly, "A4");
			Assert.AreEqual ("", sd.ServerPropertyName, "A5");
		}

		[Test]
		public void ctor4 ()
		{
			ScriptPropertyDescriptor sd = new ScriptPropertyDescriptor ("propertyName", ScriptType.Object, true, "serverPropertyName");
			Assert.AreEqual ("propertyName", sd.PropertyName, "A1");
			Assert.AreEqual ("propertyName", sd.MemberName, "A2");
			Assert.AreEqual (ScriptType.Object, sd.Type, "A3");
			Assert.IsTrue (sd.ReadOnly, "A4");
			Assert.AreEqual ("serverPropertyName", sd.ServerPropertyName, "A5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void nulltest1 ()
		{
			ScriptPropertyDescriptor sd = new ScriptPropertyDescriptor (null, ScriptType.Object);
		}

		[Test]
		public void nulltest2 ()
		{
			ScriptPropertyDescriptor sd = new ScriptPropertyDescriptor ("propertyName", ScriptType.Object, null);

			Assert.AreEqual ("", sd.ServerPropertyName, "A1");
		}

	}
}
#endif
