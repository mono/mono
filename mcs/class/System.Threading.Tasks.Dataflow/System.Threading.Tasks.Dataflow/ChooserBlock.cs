// JoinBlock.cs
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


using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	internal class ChooserBlock<T1, T2, T3>
	{
		class ChooseTarget<TMessage> : ITargetBlock<TMessage>
		{
			Action<TMessage> messageArrived;

			public ChooseTarget (Action<TMessage> messageArrived)
			{
				this.messageArrived = messageArrived;
			}

			public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
			                                           TMessage messageValue,
			                                           ISourceBlock<TMessage> source,
			                                           bool consumeToAccept)
			{
				messageArrived (messageValue);
				return DataflowMessageStatus.Accepted;
			}

			public Task Completion {
				get {
					return null;
				}
			}

			public void Complete ()
			{

			}

			public void Fault (Exception ex)
			{
			
			}
		}

		TaskCompletionSource<int> completion = new TaskCompletionSource<int> ();

		public ChooserBlock (Action<T1> action1, Action<T2> action2, Action<T3> action3, DataflowBlockOptions dataflowBlockOptions)
		{
			// TODO: take care of options and its cancellation token

			Target1 = new ChooseTarget<T1> (message => MessageArrived (0, action1, message));
			Target2 = new ChooseTarget<T2> (message => MessageArrived (1, action2, message));
			Target3 = new ChooseTarget<T3> (message => MessageArrived (2, action3, message));
		}

		void MessageArrived<TMessage> (int index, Action<TMessage> action, TMessage value)
		{
			try {
				action (value);
				completion.SetResult (index);
			} catch (Exception e) {
				completion.SetException (e);
			}
		}

		public ITargetBlock<T1> Target1 {
			get;
			private set;
		}

		public ITargetBlock<T2> Target2 {
			get;
			private set;
		}

		public ITargetBlock<T3> Target3 {
			get;
			private set;
		}

		public Task<int> Completion {
			get {
				return completion.Task;
			}
		}
	}
}

