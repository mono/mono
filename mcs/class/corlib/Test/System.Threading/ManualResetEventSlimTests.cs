#if NET_4_0
// ManualResetEventSlimTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//

using System;
using System.Threading;

using NUnit.Framework;

using MonoTests.System.Threading.Tasks;

namespace MonoTests.System.Threading
{
	
	[TestFixture]
	public class ManualResetEventSlimTests
	{
		ManualResetEventSlim mre;
		
		[SetUp]
		public void Setup()
		{
			mre = new ManualResetEventSlim();
		}
		
		[Test]
		public void IsSetTestCase()
		{
			Assert.IsFalse(mre.IsSet, "#1");
			mre.Set();
			Assert.IsTrue(mre.IsSet, "#2");
			mre.Reset();
			Assert.IsFalse(mre.IsSet, "#3");
		}
		
		[Test]
		public void WaitTest()
		{
			int count = 0;
			bool s = false;
			
			ParallelTestHelper.ParallelStressTest(mre, delegate (ManualResetEventSlim m) {
				if (Interlocked.Increment(ref count) % 2 == 0) {
					Thread.Sleep(50);
					for (int i = 0; i < 10; i++) {
						if (i % 2 == 0)
							m.Reset();
						else
							m.Set();
					}
				} else {
					m.Wait();
					s = true;
				}
			}, 2);	
			
			Assert.IsTrue(s, "#1");
			Assert.IsTrue(mre.IsSet, "#2");
		}
	}
}
#endif
