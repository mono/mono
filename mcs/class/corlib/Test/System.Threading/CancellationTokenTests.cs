// 
// CancellationTokenTests.cs
//  
// Authors:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//       Marek Safar (marek.safar@gmail.com)
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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

#if NET_4_0 || MOBILE

using System;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class CancellationTokenTests
	{
		[Test]
		public void InitedWithFalseToken ()
		{
			CancellationToken tk = new CancellationToken (false);
			Assert.IsFalse (tk.CanBeCanceled, "#1");
			Assert.IsFalse (tk.IsCancellationRequested, "#2");
		}

		[Test]
		public void InitedWithTrueToken ()
		{
			CancellationToken tk = new CancellationToken (true);
			Assert.IsTrue (tk.CanBeCanceled, "#1");
			Assert.IsTrue (tk.IsCancellationRequested, "#2");
		}

		[Test]
		public void CancellationSourceNotCanceled ()
		{
			var src = new CancellationTokenSource ();
			var tk = src.Token;

			Assert.IsTrue (tk.CanBeCanceled);
			Assert.IsFalse (tk.IsCancellationRequested);
		}

		[Test]
		public void CancellationSourceCanceled ()
		{
			var src = new CancellationTokenSource ();
			var tk = src.Token;
			src.Cancel ();

			Assert.IsTrue (tk.CanBeCanceled, "#1");
			Assert.IsTrue (tk.IsCancellationRequested, "#2");
		}

		[Test]
		public void UninitializedToken ()
		{
			var tk = new CancellationToken ();
			Assert.IsFalse (tk.CanBeCanceled);
			Assert.IsFalse (tk.IsCancellationRequested);
		}

		[Test]
		public void NoneProperty ()
		{
			var n = CancellationToken.None;
			Assert.IsFalse (n.CanBeCanceled, "#1");
			Assert.IsFalse (n.IsCancellationRequested, "#2");
			Assert.AreEqual (n, CancellationToken.None, "#3");

			n.ThrowIfCancellationRequested ();
			n.GetHashCode ();
		}

		[Test]
		public void DefaultCancellationTokenRegistration ()
		{
			var registration = new CancellationTokenRegistration ();

			// shouldn't throw
			registration.Dispose ();
		}
	}
}
#endif
