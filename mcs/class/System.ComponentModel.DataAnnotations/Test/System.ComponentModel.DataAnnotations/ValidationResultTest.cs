//
// ValidationResultTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
#if NET_4_0
	[TestFixture]
	public class ValidationResultTest
	{
		[Test]
		public void Constructor_String ()
		{
			var vr = new ValidationResult ("message");

			Assert.AreEqual ("message", vr.ErrorMessage, "#A1");
			Assert.IsNotNull (vr.MemberNames, "#A2-1");

			int count = 0;
			foreach (string m in vr.MemberNames)
				count++;
			Assert.AreEqual (0, count, "#A2-2");

			vr = new ValidationResult (null);
			Assert.AreEqual (null, vr.ErrorMessage, "#A3");
		}

		[Test]
		public void Constructor_String_IEnumerable ()
		{
			var vr = new ValidationResult ("message", null);

			Assert.AreEqual ("message", vr.ErrorMessage, "#A1");
			Assert.IsNotNull (vr.MemberNames, "#A2-1");

			int count = 0;
			foreach (string m in vr.MemberNames)
				count++;
			Assert.AreEqual (0, count, "#A2-2");

			var names = new string[] { "one", "two" };
			vr = new ValidationResult ("message", names);

			Assert.AreEqual ("message", vr.ErrorMessage, "#A1");
			Assert.IsNotNull (vr.MemberNames, "#A2-1");
			Assert.AreSame (names, vr.MemberNames, "#A2-2");

			count = 0;
			foreach (string m in vr.MemberNames)
				count++;
			Assert.AreEqual (2, count, "#A2-3");

			vr = new ValidationResult (null, null);
			Assert.AreEqual (null, vr.ErrorMessage, "#A3");
		}

		[Test]
		public void Success ()
		{
			ValidationResult success = ValidationResult.Success;

			Assert.IsNull (success, "#A1");
		}
	}
#endif
}
