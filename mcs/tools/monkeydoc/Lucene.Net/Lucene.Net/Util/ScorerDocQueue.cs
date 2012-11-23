/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


/* Derived from Mono.Lucene.Net.Util.PriorityQueue of March 2005 */
using System;

using DocIdSetIterator = Mono.Lucene.Net.Search.DocIdSetIterator;
using Scorer = Mono.Lucene.Net.Search.Scorer;

namespace Mono.Lucene.Net.Util
{
	
	/// <summary>A ScorerDocQueue maintains a partial ordering of its Scorers such that the
	/// least Scorer can always be found in constant time.  Put()'s and pop()'s
	/// require log(size) time. The ordering is by Scorer.doc().
	/// </summary>
	public class ScorerDocQueue
	{
		// later: SpansQueue for spans with doc and term positions
		private HeapedScorerDoc[] heap;
		private int maxSize;
		private int size;
		
		private class HeapedScorerDoc
		{
			private void  InitBlock(ScorerDocQueue enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ScorerDocQueue enclosingInstance;
			public ScorerDocQueue Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal Scorer scorer;
			internal int doc;
			
			internal HeapedScorerDoc(ScorerDocQueue enclosingInstance, Scorer s):this(enclosingInstance, s, s.DocID())
			{
			}
			
			internal HeapedScorerDoc(ScorerDocQueue enclosingInstance, Scorer scorer, int doc)
			{
				InitBlock(enclosingInstance);
				this.scorer = scorer;
				this.doc = doc;
			}
			
			internal virtual void  Adjust()
			{
				doc = scorer.DocID();
			}
		}
		
		private HeapedScorerDoc topHSD; // same as heap[1], only for speed
		
		/// <summary>Create a ScorerDocQueue with a maximum size. </summary>
		public ScorerDocQueue(int maxSize)
		{
			// assert maxSize >= 0;
			size = 0;
			int heapSize = maxSize + 1;
			heap = new HeapedScorerDoc[heapSize];
			this.maxSize = maxSize;
			topHSD = heap[1]; // initially null
		}
		
		/// <summary> Adds a Scorer to a ScorerDocQueue in log(size) time.
		/// If one tries to add more Scorers than maxSize
		/// a RuntimeException (ArrayIndexOutOfBound) is thrown.
		/// </summary>
		public void  Put(Scorer scorer)
		{
			size++;
			heap[size] = new HeapedScorerDoc(this, scorer);
			UpHeap();
		}
		
		/// <summary> Adds a Scorer to the ScorerDocQueue in log(size) time if either
		/// the ScorerDocQueue is not full, or not lessThan(scorer, top()).
		/// </summary>
		/// <param name="scorer">
		/// </param>
		/// <returns> true if scorer is added, false otherwise.
		/// </returns>
		public virtual bool Insert(Scorer scorer)
		{
			if (size < maxSize)
			{
				Put(scorer);
				return true;
			}
			else
			{
				int docNr = scorer.DocID();
				if ((size > 0) && (!(docNr < topHSD.doc)))
				{
					// heap[1] is top()
					heap[1] = new HeapedScorerDoc(this, scorer, docNr);
					DownHeap();
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		
		/// <summary>Returns the least Scorer of the ScorerDocQueue in constant time.
		/// Should not be used when the queue is empty.
		/// </summary>
		public Scorer Top()
		{
			// assert size > 0;
			return topHSD.scorer;
		}
		
		/// <summary>Returns document number of the least Scorer of the ScorerDocQueue
		/// in constant time.
		/// Should not be used when the queue is empty.
		/// </summary>
		public int TopDoc()
		{
			// assert size > 0;
			return topHSD.doc;
		}
		
		public float TopScore()
		{
			// assert size > 0;
			return topHSD.scorer.Score();
		}
		
		public bool TopNextAndAdjustElsePop()
		{
			return CheckAdjustElsePop(topHSD.scorer.NextDoc() != DocIdSetIterator.NO_MORE_DOCS);
		}
		
		public bool TopSkipToAndAdjustElsePop(int target)
		{
			return CheckAdjustElsePop(topHSD.scorer.Advance(target) != DocIdSetIterator.NO_MORE_DOCS);
		}
		
		private bool CheckAdjustElsePop(bool cond)
		{
			if (cond)
			{
				// see also adjustTop
				topHSD.doc = topHSD.scorer.DocID();
			}
			else
			{
				// see also popNoResult
				heap[1] = heap[size]; // move last to first
				heap[size] = null;
				size--;
			}
			DownHeap();
			return cond;
		}
		
		/// <summary>Removes and returns the least scorer of the ScorerDocQueue in log(size)
		/// time.
		/// Should not be used when the queue is empty.
		/// </summary>
		public Scorer Pop()
		{
			// assert size > 0;
			Scorer result = topHSD.scorer;
			PopNoResult();
			return result;
		}
		
		/// <summary>Removes the least scorer of the ScorerDocQueue in log(size) time.
		/// Should not be used when the queue is empty.
		/// </summary>
		private void  PopNoResult()
		{
			heap[1] = heap[size]; // move last to first
			heap[size] = null;
			size--;
			DownHeap(); // adjust heap
		}
		
		/// <summary>Should be called when the scorer at top changes doc() value.
		/// Still log(n) worst case, but it's at least twice as fast to <pre>
		/// { pq.top().change(); pq.adjustTop(); }
		/// </pre> instead of <pre>
		/// { o = pq.pop(); o.change(); pq.push(o); }
		/// </pre>
		/// </summary>
		public void  AdjustTop()
		{
			// assert size > 0;
			topHSD.Adjust();
			DownHeap();
		}
		
		/// <summary>Returns the number of scorers currently stored in the ScorerDocQueue. </summary>
		public int Size()
		{
			return size;
		}
		
		/// <summary>Removes all entries from the ScorerDocQueue. </summary>
		public void  Clear()
		{
			for (int i = 0; i <= size; i++)
			{
				heap[i] = null;
			}
			size = 0;
		}
		
		private void  UpHeap()
		{
			int i = size;
			HeapedScorerDoc node = heap[i]; // save bottom node
			int j = SupportClass.Number.URShift(i, 1);
			while ((j > 0) && (node.doc < heap[j].doc))
			{
				heap[i] = heap[j]; // shift parents down
				i = j;
				j = SupportClass.Number.URShift(j, 1);
			}
			heap[i] = node; // install saved node
			topHSD = heap[1];
		}
		
		private void  DownHeap()
		{
			int i = 1;
			HeapedScorerDoc node = heap[i]; // save top node
			int j = i << 1; // find smaller child
			int k = j + 1;
			if ((k <= size) && (heap[k].doc < heap[j].doc))
			{
				j = k;
			}
			while ((j <= size) && (heap[j].doc < node.doc))
			{
				heap[i] = heap[j]; // shift up child
				i = j;
				j = i << 1;
				k = j + 1;
				if (k <= size && (heap[k].doc < heap[j].doc))
				{
					j = k;
				}
			}
			heap[i] = node; // install saved node
			topHSD = heap[1];
		}
	}
}
