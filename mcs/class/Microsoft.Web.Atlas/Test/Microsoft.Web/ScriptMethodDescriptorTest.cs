//
// Tests for Microsoft.Web.ScriptMethodDescriptor
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
	public class ScriptMethodDescriptorTest
	{
		[Test]
		public void ctor1 ()
		{
			ScriptMethodDescriptor sd = new ScriptMethodDescriptor ("methodName");
			Assert.AreEqual ("methodName", sd.MethodName, "A1");
			Assert.AreEqual ("methodName", sd.MemberName, "A2");
			Assert.IsNotNull (sd.Parameters, "A3");
			Assert.AreEqual (0, sd.Parameters.Length, "A4");
		}

		[Test]
		public void ctor2 ()
		{
			string[] args = new string[2];
			args[0] = "arg1";
			args[1] = "arg2";

			ScriptMethodDescriptor sd = new ScriptMethodDescriptor ("methodName", args);
			Assert.AreEqual ("methodName", sd.MethodName, "A1");
			Assert.AreEqual ("methodName", sd.MemberName, "A2");
			Assert.IsNotNull (sd.Parameters, "A3");
			Assert.AreEqual (2, sd.Parameters.Length, "A4");
			Assert.AreEqual ("arg1", sd.Parameters[0], "A5");
			Assert.AreEqual ("arg2", sd.Parameters[1], "A6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void nulltest1 ()
		{
			ScriptMethodDescriptor sd = new ScriptMethodDescriptor (null);
		}

		[Test]
		public void nulltest2 ()
		{
			ScriptMethodDescriptor sd = new ScriptMethodDescriptor ("methodName", null);

			Assert.IsNotNull (sd.Parameters, "A1");
			Assert.AreEqual (0, sd.Parameters.Length, "A2");
		}

	}
}
#endif
