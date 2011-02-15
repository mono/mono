#if NET_4_0
// TaskFactoryTest.cs
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
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
using System.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	public class TaskFactoryTests
	{
		TaskFactory factory;
		
		[SetUp]
		public void Setup ()
		{
			this.factory = Task.Factory;
		}
		
		[Test]
		public void StartNewTest ()
		{
			bool result = false;
			factory.StartNew (() => result = true).Wait ();
			Assert.IsTrue (result);
		}
		
		[Test]
		public void ContinueWhenAllTest ()
		{
			bool r1 = false, r2 = false, r3 = false;
			
			Task[] tasks = new Task[3];
			tasks[0] = new Task (() => { Thread.Sleep (100); r1 = true; });
			tasks[1] = new Task (() => { Thread.Sleep (500); r2 = true; });
			tasks[2] = new Task (() => { Thread.Sleep (300); r3 = true; });
			
			bool result = false;
			
			Task cont = factory.ContinueWhenAll (tasks, (ts) => { if (r1 && r2 && r3) result = true; });
			
			foreach (Task t in tasks)
				t.Start ();
			
			cont.Wait ();
			
			Assert.IsTrue (r1, "#1");
			Assert.IsTrue (r2, "#2");
			Assert.IsTrue (r3, "#3");
			Assert.IsTrue (result, "#4");
		}
		
		[Test]
		public void ContinueWhenAnyTest ()
		{
			bool r = false, result = false, finished = false;
			
			Task[] tasks = new Task[2];
			tasks[0] = new Task (() => { Thread.Sleep (300); r = true; });
			tasks[1] = new Task (() => { SpinWait sw; while (!finished) sw.SpinOnce (); });
			//tasks[2] = new Task (() => { SpinWait sw; while (!finished) sw.SpinOnce (); });
			
			Task cont = factory.ContinueWhenAny (tasks, (t) => { if (r) result = t == tasks[0]; finished = true; });
			
			foreach (Task t in tasks)
				t.Start ();
			
			cont.Wait ();
			
			Assert.IsTrue (r, "#1");
			Assert.IsTrue (result, "#2");
			Assert.IsTrue (finished, "#3");
		}

		[Test]
		public void FromAsyncWithBeginTest ()
		{
			bool result = false;
			bool continuationTest = false;

			Func<int, int> func = (i) => { result = true; return i + 3; };
			Task<int> task = factory.FromAsync<int, int> (func.BeginInvoke, func.EndInvoke, 1, null);
			var cont = task.ContinueWith (_ => continuationTest = true, TaskContinuationOptions.ExecuteSynchronously);
			task.Wait ();
			cont.Wait ();

			Assert.IsTrue (result);
			Assert.IsTrue (continuationTest);
			Assert.AreEqual (4, task.Result);
		}

		[Test]
		public void FromAsyncWithDirectAsyncResultTest ()
		{
			bool result = false;
			bool continuationTest = false;

			Func<int, int> func = (i) => { result = true; return i + 3; };
			Task<int> task = factory.FromAsync<int> (func.BeginInvoke (1, delegate {}, null), func.EndInvoke);
			var cont = task.ContinueWith (_ => continuationTest = true, TaskContinuationOptions.ExecuteSynchronously);
			task.Wait ();
			cont.Wait ();

			Assert.IsTrue (result);
			Assert.IsTrue (continuationTest);
			Assert.AreEqual (4, task.Result);
		}

		[Test]
		public void FromAsyncWithBeginAndExceptionTest ()
		{
			bool result = false;
			bool continuationTest = false;

			Func<int, int> func = (i) => { result = true; throw new ApplicationException ("bleh"); return i + 3; };
			Task<int> task = factory.FromAsync<int, int> (func.BeginInvoke, func.EndInvoke, 1, null);
			var cont = task.ContinueWith (_ => continuationTest = true, TaskContinuationOptions.ExecuteSynchronously);
			try {
				task.Wait ();
			} catch {}
			cont.Wait ();

			Assert.IsTrue (result);
			Assert.IsTrue (continuationTest);
			Assert.IsNotNull (task.Exception);
			var agg = task.Exception;
			Assert.AreEqual (1, agg.InnerExceptions.Count);
			Assert.IsInstanceOfType (typeof (ApplicationException), agg.InnerExceptions[0]);
		}
	}
}
#endif
