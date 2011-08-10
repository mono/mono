// MessageVault.cs
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
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	/* MessageVault is used to store message value & header that have been proposed via OfferMessage
	 * so that target block can then Consume them
	 */
	internal class MessageVault<T>
	{
		class ReservationSlot
		{
			public T Data;
			public IDataflowBlock Reserved;

			public ReservationSlot (T data)
			{
				Data = data;
				Reserved = null;
			}
		}

		ConcurrentDictionary<DataflowMessageHeader, ReservationSlot> store = new ConcurrentDictionary<DataflowMessageHeader, ReservationSlot> ();

		public bool StoreMessage (DataflowMessageHeader header, T data)
		{
			if (!header.IsValid)
				throw new ArgumentException ("header", "Header is is not valid");

			return store.TryAdd (header, new ReservationSlot (data));
		}

		public T ConsumeMessage (DataflowMessageHeader header, IDataflowBlock target, out bool messageConsummed)
		{
			messageConsummed = false;
			if (!header.IsValid)
				throw new ArgumentException ("header", "Header is is not valid");
			if (target == null)
				throw new ArgumentNullException ("target");

			ReservationSlot slot;
			if (!store.TryRemove (header, out slot) || slot.Reserved != target)
				return default (T);

			messageConsummed = true;
			return slot.Data;
		}

		public bool ReserveMessage (DataflowMessageHeader header, IDataflowBlock target)
		{
			if (!header.IsValid)
				throw new ArgumentException ("header", "Header is is not valid");
			if (target == null)
				throw new ArgumentNullException ("target");

			ReservationSlot slot;
			if (!store.TryGetValue (header, out slot))
				return false;

			return Interlocked.CompareExchange (ref slot.Reserved, target, null) == null;
		}

		public void ReleaseReservation (DataflowMessageHeader header, IDataflowBlock target)
		{
			if (!header.IsValid)
				throw new ArgumentException ("header", "Header is is not valid");
			if (target == null)
				throw new ArgumentNullException ("target");

			ReservationSlot slot;
			if (!store.TryGetValue (header, out slot))
				return;

			if (Interlocked.CompareExchange (ref slot.Reserved, null, target) != target)
				throw new InvalidOperationException ("The target did not have the message reserved");
		}
	}
}

#endif
