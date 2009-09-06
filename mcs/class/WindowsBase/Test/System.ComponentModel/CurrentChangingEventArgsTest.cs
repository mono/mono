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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Brian O'Keefe (zer0keefie@gmail.com)
//

using System;
using NUnit.Framework;
using System.ComponentModel;

namespace MonoTests.System.ComponentModel {

	[TestFixture]
	public class CurrentChangingEventArgsTest {

		public CurrentChangingEventArgsTest()
		{
		}

		[Test]
		public void CurrentChangingEventArgsConstructor1Test()
		{
			CurrentChangingEventArgs args = new CurrentChangingEventArgs ();

			Assert.IsFalse (args.Cancel, "CTOR1_#1");
			Assert.IsTrue (args.IsCancelable, "CTOR1_#2");
		}

		[Test]
		public void CurrentChangingEventArgsConstructor2Test()
		{
			CurrentChangingEventArgs args = new CurrentChangingEventArgs (false);

			Assert.IsFalse (args.Cancel, "CTOR2_#1");
			Assert.IsFalse (args.IsCancelable, "CTOR2_#2");

			args = new CurrentChangingEventArgs (true);

			Assert.IsFalse (args.Cancel, "CTOR1_#3");
			Assert.IsTrue (args.IsCancelable, "CTOR1_#4");

			args.Cancel = true;

			Assert.IsTrue (args.Cancel, "CTOR1_#5");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ChangeCancelIfNotCancelableTest()
		{
			CurrentChangingEventArgs args = new CurrentChangingEventArgs (false);

			Assert.IsFalse (args.Cancel, "InvOp_#1");
			Assert.IsFalse (args.IsCancelable, "InvOp_#2");

			args.Cancel = true;
		}
	}
}
