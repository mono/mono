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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jackson Harper	jackson@ximian.com


using System;
using System.Data;
using System.Collections;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class BindingTest {

		[SetUp]
		protected virtual void SetUp ()
		{
			Console.WriteLine ("SETTING UP");
		}

		[TearDown]
		protected virtual void TearDown ()
		{
			Console.WriteLine ("TEARING DOWN");
		}

		[Test]
		public void CtorTest ()
		{
			string prop = "PROPERTY NAME";
			object data_source = new object ();
			string data_member = "DATA MEMBER";
			Binding b = new Binding (prop, data_source, data_member);

			Assert.IsNull (b.BindingManagerBase, "ctor1");
			Console.WriteLine ("MEMBER INFO:  " + b.BindingMemberInfo);
			Assert.IsNotNull (b.BindingMemberInfo, "ctor2");
			Assert.IsNull (b.Control, "ctor3");
			Assert.IsFalse (b.IsBinding, "ctor4");

			Assert.AreSame (b.PropertyName, prop, "ctor5");
			Assert.AreSame (b.DataSource, data_source, "ctor6");
		}

		[Test]
		public void CtorNullTest ()
		{
			Binding b = new Binding (null, null, null);

			Assert.IsNull (b.PropertyName, "ctornull1");
			Assert.IsNull (b.DataSource, "ctornull2");
		}
	}

}

