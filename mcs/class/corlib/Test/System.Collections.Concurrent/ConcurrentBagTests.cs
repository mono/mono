#if NET_4_0
// 
// ConcurrentBagTests.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using System.Threading;
using System.Linq;

using NUnit;
using NUnit.Framework;

namespace ParallelFxTests
{
	[TestFixture]
	public class ConcurrentBagTests
	{
		ConcurrentBag<int> bag;
		
		[SetUp]
		public void Setup ()
		{
			bag = new ConcurrentBag<int> ();
		}
		
		[Test]
		public void AddStressTest ()
		{
			CollectionStressTestHelper.AddStressTest (bag);
		}
		
		[Test]
		public void RemoveStressTest ()
		{
			CollectionStressTestHelper.RemoveStressTest (bag, CheckOrderingType.DontCare);
		}
	}
}
#endif
