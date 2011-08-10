// ExecutingMessageBox.cs
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
//
//

#if NET_4_0 || MOBILE

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	internal class ExecutingMessageBox<TInput> : MessageBox<TInput>
	{
		readonly ExecutionDataflowBlockOptions dataflowBlockOptions;
		readonly BlockingCollection<TInput> messageQueue;
		readonly Action processQueue;
		readonly CompletionHelper compHelper;

		AtomicBoolean started = new AtomicBoolean ();
		
		public ExecutingMessageBox (BlockingCollection<TInput> messageQueue,
		                            CompletionHelper compHelper,
		                            Func<bool> externalCompleteTester,
		                            Action processQueue,
		                            ExecutionDataflowBlockOptions dataflowBlockOptions) : base (messageQueue, compHelper, externalCompleteTester)
		{
			this.messageQueue = messageQueue;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.processQueue = processQueue;
			this.compHelper = compHelper;
		}

		protected override void EnsureProcessing ()
		{
			if (!started.TryRelaxedSet ())
				return;

			Task[] tasks = new Task[dataflowBlockOptions.MaxDegreeOfParallelism];
			for (int i = 0; i < tasks.Length; ++i)
				tasks[i] = Task.Factory.StartNew (processQueue);
			Task.Factory.ContinueWhenAll (tasks, (_) => {
				started.Value = false;
				// Re-run ourselves in case of a race when data is available in the end
				if (messageQueue.Count > 0)
					EnsureProcessing ();
				else if (messageQueue.IsCompleted)
					compHelper.Complete ();
			});
		}
	}
}

#endif
