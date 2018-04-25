//
// FileExtensionsAttributeTest.cs
//
// Authors:
//      Pablo Ruiz García <pablo.ruiz@gmail.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
// Copyright (C) 2013 Pablo Ruiz García
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
	public class FileExtensionsAttributeTest
	{
		[Test]
		public void IsValid ()
		{
			var sla = new FileExtensionsAttribute () {
				Extensions = "txt,jpg"
			};

			Assert.IsTrue (sla.IsValid (null), "#A1-1");
			Assert.IsFalse (sla.IsValid (String.Empty), "#A1-2");
			Assert.IsFalse (sla.IsValid ("string"), "#A1-3");
			Assert.IsTrue (sla.IsValid ("file.txt"), "#A1-4");
			Assert.IsTrue (sla.IsValid ("file.jpg"), "#A1-5");
			Assert.IsTrue (sla.IsValid ("file.xxx.txt"), "#A-6");
			Assert.IsFalse (sla.IsValid (true), "#A1-7");
			Assert.IsFalse (sla.IsValid (DateTime.Now), "#A1-8");
		}
	}
}
