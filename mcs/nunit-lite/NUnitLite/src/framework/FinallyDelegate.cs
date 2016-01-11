// ***********************************************************************
// Author:
//	Alexander Kyte <alexander.kyte@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc
//
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
// ***********************************************************************

using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework.Api;
using System.Collections.Generic;

namespace NUnit.Framework.Internal
{
	public class FinallyDelegate
	{
		// If our test spawns a thread that throws, we will bubble
		// up to the top level. To handle this, we need to have the
		// failure logic in a place that can be called from the top level.
		// The UnhandledException callback should run on a thread that isn't
		// the one dispatching the tests so this should work to ensure
		// progress in the face of exceptions on other threads
		//
		// Essentially this is a poor-man's finally clause that's
		// guaranteed to run because we pin it to the UnhandledException
		// callback

		// Because of CompositeWorkItem, we have a runtime stack of work items
		// so we need a stack of finally delegate continuations
		Stack<Tuple<TestExecutionContext, long, TestResult>> testStack;

		public FinallyDelegate () {
			this.testStack = new Stack<Tuple<TestExecutionContext, long, TestResult>>();
		}

		public void Set (TestExecutionContext context, long startTicks, TestResult result) {
			var frame = new Tuple<TestExecutionContext, long, TestResult>(context, startTicks, result);
			this.testStack.Push(frame);
		}

		public void HandleUnhandledExc (Exception ex) {
			TestExecutionContext context = this.testStack.Peek().Item1;
			context.CurrentResult.RecordException(ex);
			context.CurrentResult.ThreadCrashFail = true;
		}

		public void Complete () {
			var frame = this.testStack.Pop();

			TestExecutionContext context = frame.Item1;
			long startTicks = frame.Item2;
			TestResult result = frame.Item3;

#if (CLR_2_0 || CLR_4_0) && !SILVERLIGHT && !NETCF_2_0
			long tickCount = Stopwatch.GetTimestamp() - startTicks;
			double seconds = (double)tickCount / Stopwatch.Frequency;
			result.Duration = TimeSpan.FromSeconds(seconds);
#else
			result.Duration = DateTime.Now - Context.StartTime;
#endif

			result.AssertCount = context.AssertCount;

			context.Listener.TestFinished(result);

			context = context.Restore();
			context.AssertCount += result.AssertCount;
		}
	}
}
