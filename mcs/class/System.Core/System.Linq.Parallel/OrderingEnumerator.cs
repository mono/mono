//
// OrderingEnumerator.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
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

#if NET_4_0
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq.Parallel
{
	internal class OrderingEnumerator<T> : IEnumerator<T>
	{
		internal class SlotBucket
		{
			ConcurrentDictionary<long, T> temporaryArea = new ConcurrentDictionary<long, T> ();
			KeyValuePair<long, T>?[] stagingArea;
			
			long currentIndex;
			readonly int count;
			CountdownEvent stagingCount;
			CountdownEvent participantCount;
			CancellationTokenSource src = new CancellationTokenSource ();

			public SlotBucket (int count)
			{
				this.count = count;
				stagingCount = new CountdownEvent (count);
				participantCount = new CountdownEvent (count);
				stagingArea = new KeyValuePair<long, T>?[count];
				currentIndex = -count;
			}

			public void Add (KeyValuePair<long, T> value)
			{
				long index = value.Key;
				
				if (index >= currentIndex && index < currentIndex + count) {
					stagingArea[index % count] = value;
					stagingCount.Signal ();
				} else {
					temporaryArea.TryAdd (index, value.Value);
					if (index >= currentIndex && index < currentIndex + count) {
						T dummy;
						if (temporaryArea.TryRemove (index, out dummy)) {
							stagingArea[index % count] = value;
							stagingCount.Signal ();
						}
					}
				}
			}
			
			// Called by each worker's endAction
			public void EndParticipation ()
			{
				if (participantCount.Signal ())
					src.Cancel ();
			}

			// Called at the end with ContinueAll
			public void Stop ()
			{
				
			}

			void Skim ()
			{
				for (int i = 0; i < count; i++) {
					T temp;
					int index = i + (int)currentIndex;
					
					if (stagingArea[index % count].HasValue)
						continue;

					if (!temporaryArea.TryRemove (index, out temp))
						continue;
					
					stagingArea [index % count] = new KeyValuePair<long, T> (index, temp);
					if (stagingCount.Signal ())
						break;
				}
			}
			
			void Clean ()
			{
				for (int i = 0; i < stagingArea.Length; i++)
					stagingArea[i] = new Nullable<KeyValuePair<long, T>> ();
			}

			public IEnumerator<KeyValuePair<long, T>?> Wait ()
			{
				Clean ();
				stagingCount.Reset ();
				
				Interlocked.Add (ref currentIndex, count);

				Skim ();

				while (!stagingCount.IsSet) {
					if (!participantCount.IsSet)
						try {
							stagingCount.Wait (src.Token);
						} catch {
							Skim ();
						}

					if (participantCount.IsSet) {
						// Totally finished
						if (stagingArea[0].HasValue)
							break;
						else
							return null;
					}
				}
				
				return ((IEnumerable<KeyValuePair<long, T>?>)stagingArea).GetEnumerator ();
			}
		}

		readonly int num;
		SlotBucket slotBucket;
		
		IEnumerator<KeyValuePair<long, T>?> currEnum;
		KeyValuePair<long, T> curr;

		internal OrderingEnumerator (int num)
		{
			this.num = num;
			slotBucket = new SlotBucket (num);
		}

		public void Dispose ()
		{

		}

		public void Reset ()
		{

		}

		public bool MoveNext ()
		{
			do {
				while (currEnum == null || !currEnum.MoveNext ()) {
					if (currEnum != null)
						currEnum.Dispose ();
					if ((currEnum = slotBucket.Wait ()) == null)
						return false;
				}
			} while (!currEnum.Current.HasValue);

			curr = currEnum.Current.Value;

			return true;
		}

		public T Current {
			get {
				return curr.Value;
			}
		}

		object IEnumerator.Current {
			get {
				return curr.Value;
			}
		}
		
		public void Add (KeyValuePair<long, T> value)
		{
			slotBucket.Add (value);
		}
			
		// Called by each worker's endAction
		public void EndParticipation ()
		{
			slotBucket.EndParticipation ();
		}
		
		// Called at the end with ContinueAll
		public void Stop ()
		{
			slotBucket.Stop ();
		}
	}
}
#endif
