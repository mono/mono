//
// RequiredAttributeTest.cs
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
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
	[TestFixture]
	public class RequiredAttributeTest
	{
		[Test]
		public void IsRequired ()
		{
			var attr = new RequiredAttribute ();
			Assert.IsFalse (attr.IsValid (null), "#A1");
			Assert.IsFalse (attr.IsValid (String.Empty), "#A2");
			Assert.IsTrue (attr.IsValid ("string"), "#A3");
			Assert.IsTrue (attr.IsValid (1), "#A4");
#if NET_4_0
			attr.AllowEmptyStrings = true;
			Assert.IsTrue (attr.IsValid (String.Empty), "#A5");
#endif
		}
#if NET_4_0
		[Test]
		public void AllowEmptyStrings ()
		{
			var attr = new RequiredAttribute ();

			Assert.IsFalse (attr.AllowEmptyStrings, "#A1");
			attr.AllowEmptyStrings = true;
			Assert.IsTrue (attr.AllowEmptyStrings, "#A2");
		}
#endif
	}
}
