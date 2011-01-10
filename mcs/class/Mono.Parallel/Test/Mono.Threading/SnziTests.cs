#if NET_4_0
// 
// SnziTests.cs
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

namespace MonoTests.System.Threading.Tasks
{
	[TestFixtureAttribute]
	public class SnziTests
	{
		Snzi snzi;
		
		[SetUpAttribute]
		public void Setup ()
		{
			snzi = new Snzi ();
		}
		
		[Test]
		public void InitialTest ()
		{
			Assert.IsTrue (snzi.IsSet, "#1");
			
		}
		
		[Test]
		public void SimpleOperationTest ()
		{
			snzi.Increment ();

			snzi.Decrement ();
			
			Assert.IsTrue (snzi.IsSet, "#1");
			
		}
		
		[Test]
		public void SimpleZeroTest ()
		{
			for (int i = 0; i < 10; i++) {
				if (i % 2 == 0)
					snzi.Increment ();
				else
					snzi.Decrement ();
			}
			
			Assert.IsTrue (snzi.IsSet, "#1");
		}
		
		[Test]
		public void SimpleNonZeroTest ()
		{
			snzi.Increment ();
			
			for (int i = 0; i < 20; i++) {
				if (i % 2 == 0)
					snzi.Increment ();
				else
					snzi.Decrement ();
				if (i % 5 == 0)
					Thread.Sleep (0);
			}
			
			Assert.IsFalse (snzi.IsSet, "#1");
		}
		
		[Test]
		public void StressZeroTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				int times = 0;
				
				ParallelTestHelper.ParallelStressTest (snzi, (s) => {
					int t = Interlocked.Increment (ref times);
					
					for (int i = 0; i < 20; i++) {
						if (i % 2 == 0)
							snzi.Increment ();
						else
							snzi.Decrement ();
						if (i % (3 * t) == 0)
							Thread.Sleep (0);
					}
				});
			
				Assert.IsTrue (snzi.IsSet, "#1");
			});
		}
		
		[Test]
		public void StressNonZeroTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				ParallelTestHelper.ParallelStressTest (snzi, (s) => {
					snzi.Increment ();
					for (int i = 0; i < 1; i++) {
						if (i % 2 == 0)
							snzi.Increment ();
						else
							snzi.Decrement ();
					}
				});
			
				Assert.IsFalse (snzi.IsSet, "#1");
			});
		}
	}
}
#endif