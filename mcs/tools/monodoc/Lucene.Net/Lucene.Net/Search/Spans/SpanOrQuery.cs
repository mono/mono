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
	
	/// <summary>Matches the union of its clauses.</summary>
	[Serializable]
	public class SpanOrQuery : SpanQuery
	{
		private class AnonymousClassSpans : Spans
		{
			public AnonymousClassSpans(Monodoc.Lucene.Net.Index.IndexReader reader, SpanOrQuery enclosingInstance)
			{
				InitBlock(reader, enclosingInstance);
			}
			private void  InitBlock(Monodoc.Lucene.Net.Index.IndexReader reader, SpanOrQuery enclosingInstance)
			{
				this.reader = reader;
				this.enclosingInstance = enclosingInstance;
				all = new System.Collections.ArrayList(Enclosing_Instance.clauses.Count);
				queue = new SpanQueue(enclosingInstance, Enclosing_Instance.clauses.Count);
				System.Collections.IEnumerator i = Enclosing_Instance.clauses.GetEnumerator();
				while (i.MoveNext())
				{
					// initialize all
					all.Add(((SpanQuery) i.Current).GetSpans(reader));
				}
			}
			private Monodoc.Lucene.Net.Index.IndexReader reader;
			private SpanOrQuery enclosingInstance;
			public SpanOrQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private System.Collections.IList all;
			private SpanQueue queue;
			
			private bool firstTime = true;
			
			public virtual bool Next()
			{
				if (firstTime)
				{
					// first time -- initialize
					for (int i = 0; i < all.Count; i++)
					{
						Spans spans = (Spans) all[i];
						if (spans.Next())
						{
							// move to first entry
							queue.Put(spans); // build queue
						}
						else
						{
							all.RemoveAt(i--);
						}
					}
					firstTime = false;
					return queue.Size() != 0;
				}
				
				if (queue.Size() == 0)
				{
					// all done
					return false;
				}
				
				if (Top().Next())
				{
					// move to next
					queue.AdjustTop();
					return true;
				}
				
				all.Remove(queue.Pop()); // exhausted a clause
				
				return queue.Size() != 0;
			}
			
			private Spans Top()
			{
				return (Spans) queue.Top();
			}
			
			public virtual bool SkipTo(int target)
			{
				if (firstTime)
				{
					for (int i = 0; i < all.Count; i++)
					{
						Spans spans = (Spans) all[i];
						if (spans.SkipTo(target))
						{
							// skip each spans in all
							queue.Put(spans); // build queue
						}
						else
						{
							all.RemoveAt(i--);
						}
					}
					firstTime = false;
				}
				else
				{
					while (queue.Size() != 0 && Top().Doc() < target)
					{
						if (Top().SkipTo(target))
						{
							queue.AdjustTop();
						}
						else
						{
							all.Remove(queue.Pop());
						}
					}
				}
				
				return queue.Size() != 0;
			}
			
			public virtual int Doc()
			{
				return Top().Doc();
			}
			public virtual int Start()
			{
				return Top().Start();
			}
			public virtual int End()
			{
				return Top().End();
			}
			
			public override System.String ToString()
			{
				return "spans(" + Enclosing_Instance + ")@" + (firstTime?"START":(queue.Size() > 0?(Doc() + ":" + Start() + "-" + End()):"END"));
			}
		}
		private System.Collections.ArrayList clauses;
		private System.String field;
		
		/// <summary>Construct a SpanOrQuery merging the provided clauses. </summary>
		public SpanOrQuery(SpanQuery[] clauses)
		{
			
			// copy clauses array into an ArrayList
			this.clauses = new System.Collections.ArrayList(clauses.Length);
			for (int i = 0; i < clauses.Length; i++)
			{
				SpanQuery clause = clauses[i];
				if (i == 0)
				{
					// check Field
					field = clause.GetField();
				}
				else if (!clause.GetField().Equals(field))
				{
					throw new System.ArgumentException("Clauses must have same Field.");
				}
				this.clauses.Add(clause);
			}
		}
		
		/// <summary>Return the clauses whose spans are matched. </summary>
		public virtual SpanQuery[] GetClauses()
		{
            return (SpanQuery[]) clauses.ToArray(typeof(SpanQuery[]));
		}
		
		public override System.String GetField()
		{
			return field;
		}
		
		public override System.Collections.ICollection GetTerms()
		{
			System.Collections.ArrayList terms = new System.Collections.ArrayList();
			System.Collections.IEnumerator i = clauses.GetEnumerator();
			while (i.MoveNext())
			{
				SpanQuery clause = (SpanQuery) i.Current;
				terms.AddRange(clause.GetTerms());
			}
			return terms;
		}
		
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("spanOr([");
			System.Collections.IEnumerator i = clauses.GetEnumerator();
			while (i.MoveNext())
			{
				SpanQuery clause = (SpanQuery) i.Current;
				buffer.Append(clause.ToString(field));
				if (i.MoveNext())
				{
					buffer.Append(", ");
				}
			}
			buffer.Append("])");
			return buffer.ToString();
		}
		
		private class SpanQueue:PriorityQueue
		{
			private void  InitBlock(SpanOrQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private SpanOrQuery enclosingInstance;
			public SpanOrQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public SpanQueue(SpanOrQuery enclosingInstance, int size)
			{
				InitBlock(enclosingInstance);
				Initialize(size);
			}
			
			public override bool LessThan(System.Object o1, System.Object o2)
			{
				Spans spans1 = (Spans) o1;
				Spans spans2 = (Spans) o2;
				if (spans1.Doc() == spans2.Doc())
				{
					if (spans1.Start() == spans2.Start())
					{
						return spans1.End() < spans2.End();
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
		
		
		public override Spans GetSpans(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			if (clauses.Count == 1)
			// optimize 1-clause case
				return ((SpanQuery) clauses[0]).GetSpans(reader);
			
			return new AnonymousClassSpans(reader, this);
		}
	}
}
