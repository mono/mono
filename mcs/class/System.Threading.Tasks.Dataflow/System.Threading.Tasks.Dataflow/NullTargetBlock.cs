// NullTargetBlock.cs
//
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

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// Target block returned by <see cref="DataflowBlock.NullTarget{TInput}"/>.
	/// </summary>
	class NullTargetBlock<TInput> : ITargetBlock<TInput> {
		public NullTargetBlock ()
		{
			Completion = new TaskCompletionSource<bool> ().Task;
		}

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, TInput messageValue,
			ISourceBlock<TInput> source, bool consumeToAccept)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (consumeToAccept && source == null)
				throw new ArgumentException (
					"consumeToAccept may only be true if provided with a non-null source.",
					"consumeToAccept");

			if (consumeToAccept) {
				if (!source.ReserveMessage (messageHeader, this))
					return DataflowMessageStatus.NotAvailable;
				bool consummed;
				source.ConsumeMessage (messageHeader, this, out consummed);
				if (!consummed)
					return DataflowMessageStatus.NotAvailable;
			}

			return DataflowMessageStatus.Accepted;
		}

		public Task Completion { get; private set; }

		public void Complete ()
		{
		}

		public void Fault (Exception exception)
		{
		}
	}
}