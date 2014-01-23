// PassingMessageBox.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
// Copyright (c) 2012 Petr Onderka
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

using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// Message box for blocks that don't need any special processing of incoming items.
	/// </summary>
	class PassingMessageBox<TInput> : MessageBox<TInput> {
		readonly Action<bool> processQueue;

		public PassingMessageBox (
			ITargetBlock<TInput> target, BlockingCollection<TInput> messageQueue,
			CompletionHelper compHelper, Func<bool> externalCompleteTester,
			Action<bool> processQueue, DataflowBlockOptions dataflowBlockOptions,
			bool greedy = true, Func<bool> canAccept = null)
			: base (target, messageQueue, compHelper, externalCompleteTester,
				dataflowBlockOptions, greedy, canAccept)
		{
			this.processQueue = processQueue;
		}

		/// <summary>
		/// Makes sure the input queue is processed the way it needs to.
		/// Executes synchronously, so shouldn't cause any long processing.
		/// </summary>
		/// <param name="newItem">Was new item just added?</param>
		protected override void EnsureProcessing (bool newItem)
		{
			processQueue (newItem);
		}
	}
}