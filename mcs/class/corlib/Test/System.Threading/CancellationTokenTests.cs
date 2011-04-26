#if NET_4_0
// 
// CancellationTokenTests.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Threading
{
	[TestFixtureAttribute]
	public class CancellationTokenTests
	{
		[Test]
		public void TestInitedWithFalseToken ()
		{
			CancellationToken tk = new CancellationToken (false);
			Assert.IsFalse (tk.CanBeCanceled, "#1");
			Assert.IsFalse (tk.IsCancellationRequested, "#2");
		}

		[Test]
		public void TestInitedWithTrueToken ()
		{
			CancellationToken tk = new CancellationToken (true);
			Assert.IsTrue (tk.CanBeCanceled, "#1");
			Assert.IsTrue (tk.IsCancellationRequested, "#2");
		}

		[Test]
		public void TestWithCancellationSourceNotCanceled ()
		{
			var src = new CancellationTokenSource ();
			var tk = src.Token;

			Assert.IsTrue (tk.CanBeCanceled);
			Assert.IsFalse (tk.IsCancellationRequested);
		}

		[Test]
		public void TestWithCancellationSourceCanceled ()
		{
			var src = new CancellationTokenSource ();
			var tk = src.Token;
			src.Cancel ();

			Assert.IsTrue (tk.CanBeCanceled);
			Assert.IsTrue (tk.IsCancellationRequested);
		}
	}
}
#endif
