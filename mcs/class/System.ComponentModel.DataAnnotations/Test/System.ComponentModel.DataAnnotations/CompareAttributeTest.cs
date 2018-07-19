//
// CompareAttributeTest.cs
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
	public class CompareAttributeTest
	{
		public class TestModel {
			public string A { get; set; }
			[Display(Name = "TheB")]
			public string B { get; set; }
		}

		[Test]
		public void GetValidationResult ()
		{
			var sla = new CompareAttribute ("B");
			var obj = new TestModel { A = "x", B = "x" };
			var ctx = new ValidationContext(obj, null, null);

			Assert.IsNotNull (sla.GetValidationResult (null, ctx), "#A1-1");
			Assert.IsNotNull (sla.GetValidationResult (String.Empty, ctx), "#A1-2");
			Assert.IsNotNull (sla.GetValidationResult (obj, ctx), "#A1-3");
			Assert.IsNull (sla.GetValidationResult (obj.A, ctx), "#A1-4");

			obj = new TestModel { A = "x", B = "n" };

			Assert.IsNotNull (sla.GetValidationResult (null, ctx), "#B-1");
			Assert.IsNotNull (sla.GetValidationResult (obj, ctx), "#B-2");
			Assert.IsNotNull (sla.GetValidationResult (true, ctx), "#B-3");
			Assert.IsNotNull (sla.GetValidationResult (DateTime.Now, ctx), "#B-4");
		}
	}
}
