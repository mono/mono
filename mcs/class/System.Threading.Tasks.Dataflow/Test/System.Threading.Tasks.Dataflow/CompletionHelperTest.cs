// 
// CompletionHelperTest.cs
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
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow
{
	[TestFixture]
	public class CompletionHelperTest
	{
		CompletionHelper helper;

		[SetUp]
		public void Setup ()
		{
			helper = CompletionHelper.GetNew (null);
		}

		[Test]
		public void InitialStateTest ()
		{
			Task completed = helper.Completion;

			Assert.IsNotNull (completed);
			Assert.IsFalse (completed.IsCompleted);
		}

		[Test]
		public void FaultedTest ()
		{
			Exception ex = new ApplicationException ("Foobar");
			helper.RequestFault (ex);
			Task completed = helper.Completion;

			Assert.IsNotNull (completed);
			Assert.IsTrue (completed.IsCompleted);
			Assert.AreEqual (TaskStatus.Faulted, completed.Status);
			Assert.AreEqual (ex, completed.Exception.InnerExceptions.First ());
		}

		[Test]
		public void CompleteTest ()
		{
			helper.Complete ();
			Task completed = helper.Completion;

			Assert.IsNotNull (completed);
			Assert.IsTrue (completed.IsCompleted);
			Assert.IsFalse (completed.IsFaulted);
			Assert.IsFalse (completed.IsCanceled);
		}
	}
}
