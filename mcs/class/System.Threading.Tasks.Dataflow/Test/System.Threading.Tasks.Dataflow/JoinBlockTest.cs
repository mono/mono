// 
// JoinBlockTest.cs
//  
// Author:
//       Jérémie "garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2011 Jérémie "garuma" Laval
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow
{
	[TestFixture]
	public class JoinBlockTest
	{
		[Test]
		public void BasicUsageTest ()
		{
			Tuple<int, int> tuple = null;

			var ablock = new ActionBlock<Tuple<int, int>> (t => tuple = t);
			var block = new JoinBlock<int, int> ();
			block.LinkTo (ablock);

			block.Target1.Post (42);

			Thread.Sleep (1600);
			Assert.IsNull (tuple);

			block.Target2.Post (24);

			Thread.Sleep (1600);
			Assert.IsNotNull (tuple);
			Assert.AreEqual (42, tuple.Item1);
			Assert.AreEqual (24, tuple.Item2);
		}
	}
}
