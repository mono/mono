// 
// BroadcastBlockTest.cs
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
	public class BroadcastBlockTest
	{
		[Test]
		public void BasicUsageTest ()
		{
			bool act1 = false, act2 = false;
			var evt = new CountdownEvent (2);

			var broadcast = new BroadcastBlock<int> (null);
			var action1 = new ActionBlock<int> (i => { act1 = i == 42; evt.Signal (); });
			var action2 = new ActionBlock<int> (i => { act2 = i == 42; evt.Signal (); });

			broadcast.LinkTo (action1);
			broadcast.LinkTo (action2);

			Assert.IsTrue (broadcast.Post (42));

			evt.Wait ();

			Assert.IsTrue (act1);
			Assert.IsTrue (act2);
		}

		[Test]
		public void CloningTest ()
		{
			object act1 = null, act2 = null;
			var evt = new CountdownEvent (2);

			object source = new object ();
			var broadcast = new BroadcastBlock<object> (o => new object ());
			var action1 = new ActionBlock<object> (i => { act1 = i; evt.Signal (); });
			var action2 = new ActionBlock<object> (i => { act2 = i; evt.Signal (); });

			broadcast.LinkTo (action1);
			broadcast.LinkTo (action2);

			Assert.IsTrue (broadcast.Post (source));

			evt.Wait ();

			Assert.IsNotNull (act1);
			Assert.IsNotNull (act2);

			Assert.IsFalse (source.Equals (act1));
			Assert.IsFalse (source.Equals (act2));
			Assert.IsFalse (act2.Equals (act1));
		}
	}
}
