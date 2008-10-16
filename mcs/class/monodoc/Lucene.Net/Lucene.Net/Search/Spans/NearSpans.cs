/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using Monodoc.Lucene.Net.Index;
using PriorityQueue = Monodoc.Lucene.Net.Util.PriorityQueue;
namespace Monodoc.Lucene.Net.Search.Spans
{
	
	class NearSpans : Spans
	{
		private SpanNearQuery query;
		
		private System.Collections.IList ordered = new System.Collections.ArrayList(); // spans in query order
		private int slop; // from query
		private bool inOrder; // from query
		
		private SpansCell first; // linked list of spans
		private SpansCell last; // sorted by doc only
		
		private int totalLength; // sum of current lengths
		
		private CellQueue queue; // sorted queue of spans
		private SpansCell max; // max element in queue
		
		private bool more = true; // true iff not done
		private bool firstTime = true; // true before first next()
		
		private class CellQueue : PriorityQueue
		{
			private void  InitBlock(NearSpans enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private NearSpans enclosingInstance;
			public NearSpans Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public CellQueue(NearSpans enclosingInstance, int size)
			{
				InitBlock(enclosingInstance);
				Initialize(size);
			}
			
			public override bool LessThan(System.Object o1, System.Object o2)
			{
				SpansCell spans1 = (SpansCell) o1;
				SpansCell spans2 = (SpansCell) o2;
				if (spans1.Doc() == spans2.Doc())
				{
					if (spans1.Start() == spans2.Start())
					{
						if (spans1.End() == spans2.End())
						{
							return spans1.index > spans2.index;
						}
						else
						{
							return spans1.End() < spans2.End();
						}
					}
					else
					{
						return spans1.Start() < spans2.Start();
					}
				}
				else
				{
					return spans1.Doc() < spans2.Doc();
				}
			}
		}
		
		
		/// <summary>Wraps a Spans, and can be used to form a linked list. </summary>
		private class SpansCell : Spans
		{
			private void  InitBlock(NearSpans enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private NearSpans enclosingInstance;
			public NearSpans Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			private Spans spans;
			public SpansCell next;
			private int length = - 1;
			public int index;
			
            public SpansCell(NearSpans enclosingInstance, Spans spans, int index)
			{
				InitBlock(enclosingInstance);
                this.spans = spans;
                this.index = index;
            }
			
			public virtual bool Next()
			{
				if (length != - 1)
				// subtract old length
					Enclosing_Instance.totalLength -= length;
				
				bool more = spans.Next(); // move to next
				
				if (more)
				{
					length = End() - Start(); // compute new length
					Enclosing_Instance.totalLength += length; // add new length to total
					
					if (Enclosing_Instance.max == null || Doc() > Enclosing_Instance.max.Doc() || (Doc() == Enclosing_Instance.max.Doc() && End() > Enclosing_Instance.max.End()))
						Enclosing_Instance.max = this;
				}
				
				return more;
			}
			
			public virtual bool SkipTo(int target)
			{
				if (length != - 1)
				// subtract old length
					Enclosing_Instance.totalLength -= length;
				
				bool more = spans.SkipTo(target); // skip
				
				if (more)
				{
					length = End() - Start(); // compute new length
					Enclosing_Instance.totalLength += length; // add new length to total
					
					if (Enclosing_Instance.max == null || Doc() > Enclosing_Instance.max.Doc() || (Doc() == Enclosing_Instance.max.Doc() && End() > Enclosing_Instance.max.End()))
						Enclosing_Instance.max = this;
				}
				
				return more;
			}
			
			public virtual int Doc()
			{
				return spans.Doc();
			}
			public virtual int Start()
			{
				return spans.Start();
			}
			public virtual int End()
			{
				return spans.End();
			}
			
			public override System.String ToString()
			{
				return spans.ToString() + "#" + index;
			}
		}
		
		public NearSpans(SpanNearQuery query, Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			this.query = query;
			this.slop = query.GetSlop();
			this.inOrder = query.IsInOrder();
			
			SpanQuery[] clauses = query.GetClauses(); // initialize spans & list
			queue = new CellQueue(this, clauses.Length);
			for (int i = 0; i < clauses.Length; i++)
			{
				SpansCell cell = new SpansCell(this, clauses[i].GetSpans(reader), i);
				ordered.Add(cell); // add to ordered
			}
		}
		
		public virtual bool Next()
		{
			if (firstTime)
			{
				InitList(true);
				ListToQueue(); // initialize queue
				firstTime = false;
			}
			else if (more)
			{
				more = Min().Next(); // trigger further scanning
				if (more)
					queue.AdjustTop(); // maintain queue
			}
			
			while (more)
			{
				
				bool queueStale = false;
				
				if (Min().Doc() != max.Doc())
				{
					// maintain list
					QueueToList();
					queueStale = true;
				}
				
				// skip to doc w/ all clauses
				
				while (more && first.Doc() < last.Doc())
				{
					more = first.SkipTo(last.Doc()); // skip first upto last
					FirstToLast(); // and move it to the end
					queueStale = true;
				}
				
				if (!more)
					return false;
				
				// found doc w/ all clauses
				
				if (queueStale)
				{
					// maintain the queue
					ListToQueue();
					queueStale = false;
				}
				
				if (AtMatch())
					return true;
				
				// trigger further scanning
				if (inOrder && CheckSlop())
				{
					/* There is a non ordered match within slop and an ordered match is needed. */
					more = FirstNonOrderedNextToPartialList();
					if (more)
					{
						PartialListToQueue();
					}
				}
				else
				{
					more = Min().Next();
					if (more)
					{
						queue.AdjustTop(); // maintain queue
					}
				}
			}
			return false; // no more matches
		}
		
		public virtual bool SkipTo(int target)
		{
			if (firstTime)
			{
				// initialize
				InitList(false);
				for (SpansCell cell = first; more && cell != null; cell = cell.next)
				{
					more = cell.SkipTo(target); // skip all
				}
				if (more)
				{
					ListToQueue();
				}
				firstTime = false;
			}
			else
			{
				// normal case
				while (more && Min().Doc() < target)
				{
					// skip as needed
					more = Min().SkipTo(target);
					if (more)
						queue.AdjustTop();
				}
			}
			if (more)
			{
				
				if (AtMatch())
				// at a match?
					return true;
				
				return Next(); // no, scan
			}
			
			return false;
		}
		
		private SpansCell Min()
		{
			return (SpansCell) queue.Top();
		}
		
		public virtual int Doc()
		{
			return Min().Doc();
		}
		public virtual int Start()
		{
			return Min().Start();
		}
		public virtual int End()
		{
			return max.End();
		}
		
		
		public override System.String ToString()
		{
			return "spans(" + query.ToString() + ")@" + (firstTime?"START":(more?(Doc() + ":" + Start() + "-" + End()):"END"));
		}
		
		private void  InitList(bool next)
		{
			for (int i = 0; more && i < ordered.Count; i++)
			{
				SpansCell cell = (SpansCell) ordered[i];
				if (next)
					more = cell.Next(); // move to first entry
				if (more)
				{
					AddToList(cell); // add to list
				}
			}
		}
		
		private void  AddToList(SpansCell cell)
		{
			if (last != null)
			{
				// add next to end of list
				last.next = cell;
			}
			else
				first = cell;
			last = cell;
			cell.next = null;
		}
		
		private void  FirstToLast()
		{
			last.next = first; // move first to end of list
			last = first;
			first = first.next;
			last.next = null;
		}
		
		private void  QueueToList()
		{
			last = first = null;
			while (queue.Top() != null)
			{
				AddToList((SpansCell) queue.Pop());
			}
		}
		
		private bool FirstNonOrderedNextToPartialList()
		{
			/* Creates a partial list consisting of first non ordered and earlier.
			* Returns first non ordered .next().
			*/
			last = first = null;
			int orderedIndex = 0;
			while (queue.Top() != null)
			{
				SpansCell cell = (SpansCell) queue.Pop();
				AddToList(cell);
				if (cell.index == orderedIndex)
				{
					orderedIndex++;
				}
				else
				{
					return cell.Next();
					// FIXME: continue here, rename to eg. checkOrderedMatch():
					// when checkSlop() and not ordered, repeat cell.next().
					// when checkSlop() and ordered, add to list and repeat queue.pop()
					// without checkSlop(): no match, rebuild the queue from the partial list.
					// When queue is empty and checkSlop() and ordered there is a match.
				}
			}
			throw new System.SystemException("Unexpected: ordered");
		}
		
		private void  ListToQueue()
		{
			queue.Clear(); // rebuild queue
			PartialListToQueue();
		}
		
		private void  PartialListToQueue()
		{
			for (SpansCell cell = first; cell != null; cell = cell.next)
			{
				queue.Put(cell); // add to queue from list
			}
		}
		
		private bool AtMatch()
		{
			return (Min().Doc() == max.Doc()) && CheckSlop() && (!inOrder || MatchIsOrdered());
		}
		
		private bool CheckSlop()
		{
			int matchLength = max.End() - Min().Start();
			return (matchLength - totalLength) <= slop;
		}
		
		private bool MatchIsOrdered()
		{
			int lastStart = - 1;
			for (int i = 0; i < ordered.Count; i++)
			{
				int start = ((SpansCell) ordered[i]).Start();
				if (!(start > lastStart))
					return false;
				lastStart = start;
			}
			return true;
		}
	}
}