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

using System;

using IndexReader = Mono.Lucene.Net.Index.IndexReader;

namespace Mono.Lucene.Net.Search.Spans
{
	
	/// <summary>A Spans that is formed from the ordered subspans of a SpanNearQuery
	/// where the subspans do not overlap and have a maximum slop between them.
	/// <p/>
	/// The formed spans only contains minimum slop matches.<br/>
	/// The matching slop is computed from the distance(s) between
	/// the non overlapping matching Spans.<br/>
	/// Successive matches are always formed from the successive Spans
	/// of the SpanNearQuery.
	/// <p/>
	/// The formed spans may contain overlaps when the slop is at least 1.
	/// For example, when querying using
	/// <pre>t1 t2 t3</pre>
	/// with slop at least 1, the fragment:
	/// <pre>t1 t2 t1 t3 t2 t3</pre>
	/// matches twice:
	/// <pre>t1 t2 .. t3      </pre>
	/// <pre>      t1 .. t2 t3</pre>
	/// 
	/// 
	/// Expert:
	/// Only public for subclassing.  Most implementations should not need this class
	/// </summary>
	public class NearSpansOrdered:Spans
	{
		internal class AnonymousClassComparator : System.Collections.IComparer
		{
			public AnonymousClassComparator(NearSpansOrdered enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(NearSpansOrdered enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private NearSpansOrdered enclosingInstance;
			public NearSpansOrdered Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public virtual int Compare(System.Object o1, System.Object o2)
			{
				return ((Spans) o1).Doc() - ((Spans) o2).Doc();
			}
		}
		private void  InitBlock()
		{
			spanDocComparator = new AnonymousClassComparator(this);
		}
		private int allowedSlop;
		private bool firstTime = true;
		private bool more = false;
		
		/// <summary>The spans in the same order as the SpanNearQuery </summary>
		private Spans[] subSpans;
		
		/// <summary>Indicates that all subSpans have same doc() </summary>
		private bool inSameDoc = false;
		
		private int matchDoc = - 1;
		private int matchStart = - 1;
		private int matchEnd = - 1;
		private System.Collections.Generic.List<byte[]> matchPayload;
		
		private Spans[] subSpansByDoc;
		private System.Collections.IComparer spanDocComparator;
		
		private SpanNearQuery query;
		private bool collectPayloads = true;
		
		public NearSpansOrdered(SpanNearQuery spanNearQuery, IndexReader reader):this(spanNearQuery, reader, true)
		{
		}
		
		public NearSpansOrdered(SpanNearQuery spanNearQuery, IndexReader reader, bool collectPayloads)
		{
			InitBlock();
			if (spanNearQuery.GetClauses().Length < 2)
			{
				throw new System.ArgumentException("Less than 2 clauses: " + spanNearQuery);
			}
			this.collectPayloads = collectPayloads;
			allowedSlop = spanNearQuery.GetSlop();
			SpanQuery[] clauses = spanNearQuery.GetClauses();
			subSpans = new Spans[clauses.Length];
			matchPayload = new System.Collections.Generic.List<byte[]>();
			subSpansByDoc = new Spans[clauses.Length];
			for (int i = 0; i < clauses.Length; i++)
			{
				subSpans[i] = clauses[i].GetSpans(reader);
				subSpansByDoc[i] = subSpans[i]; // used in toSameDoc()
			}
			query = spanNearQuery; // kept for toString() only.
		}
		
		// inherit javadocs
		public override int Doc()
		{
			return matchDoc;
		}
		
		// inherit javadocs
		public override int Start()
		{
			return matchStart;
		}
		
		// inherit javadocs
		public override int End()
		{
			return matchEnd;
		}
		
		public virtual Spans[] GetSubSpans()
		{
			return subSpans;
		}
		
		// TODO: Remove warning after API has been finalized
		// TODO: Would be nice to be able to lazy load payloads
		public override System.Collections.Generic.ICollection<byte[]> GetPayload()
		{
			return matchPayload;
		}
		
		// TODO: Remove warning after API has been finalized
		public override bool IsPayloadAvailable()
		{
			return (matchPayload.Count == 0) == false;
		}
		
		// inherit javadocs
		public override bool Next()
		{
			if (firstTime)
			{
				firstTime = false;
				for (int i = 0; i < subSpans.Length; i++)
				{
					if (!subSpans[i].Next())
					{
						more = false;
						return false;
					}
				}
				more = true;
			}
			if (collectPayloads)
			{
				matchPayload.Clear();
			}
			return AdvanceAfterOrdered();
		}
		
		// inherit javadocs
		public override bool SkipTo(int target)
		{
			if (firstTime)
			{
				firstTime = false;
				for (int i = 0; i < subSpans.Length; i++)
				{
					if (!subSpans[i].SkipTo(target))
					{
						more = false;
						return false;
					}
				}
				more = true;
			}
			else if (more && (subSpans[0].Doc() < target))
			{
				if (subSpans[0].SkipTo(target))
				{
					inSameDoc = false;
				}
				else
				{
					more = false;
					return false;
				}
			}
			if (collectPayloads)
			{
				matchPayload.Clear();
			}
			return AdvanceAfterOrdered();
		}
		
		/// <summary>Advances the subSpans to just after an ordered match with a minimum slop
		/// that is smaller than the slop allowed by the SpanNearQuery.
		/// </summary>
		/// <returns> true iff there is such a match.
		/// </returns>
		private bool AdvanceAfterOrdered()
		{
			while (more && (inSameDoc || ToSameDoc()))
			{
				if (StretchToOrder() && ShrinkToAfterShortestMatch())
				{
					return true;
				}
			}
			return false; // no more matches
		}
		
		
		/// <summary>Advance the subSpans to the same document </summary>
		private bool ToSameDoc()
		{
			System.Array.Sort(subSpansByDoc, spanDocComparator);
			int firstIndex = 0;
			int maxDoc = subSpansByDoc[subSpansByDoc.Length - 1].Doc();
			while (subSpansByDoc[firstIndex].Doc() != maxDoc)
			{
				if (!subSpansByDoc[firstIndex].SkipTo(maxDoc))
				{
					more = false;
					inSameDoc = false;
					return false;
				}
				maxDoc = subSpansByDoc[firstIndex].Doc();
				if (++firstIndex == subSpansByDoc.Length)
				{
					firstIndex = 0;
				}
			}
			for (int i = 0; i < subSpansByDoc.Length; i++)
			{
				System.Diagnostics.Debug.Assert((subSpansByDoc [i].Doc() == maxDoc)
					, "NearSpansOrdered.toSameDoc() spans " + subSpansByDoc [0] 
					+ "\n at doc " + subSpansByDoc [i].Doc() 
					+ ", but should be at " + maxDoc);
			}
			inSameDoc = true;
			return true;
		}
		
		/// <summary>Check whether two Spans in the same document are ordered.</summary>
		/// <param name="spans1">
		/// </param>
		/// <param name="spans2">
		/// </param>
		/// <returns> true iff spans1 starts before spans2
		/// or the spans start at the same position,
		/// and spans1 ends before spans2.
		/// </returns>
		internal static bool DocSpansOrdered(Spans spans1, Spans spans2)
		{
			System.Diagnostics.Debug.Assert(spans1.Doc() == spans2.Doc(), "doc1 " + spans1.Doc() + " != doc2 " + spans2.Doc());
			int start1 = spans1.Start();
			int start2 = spans2.Start();
			/* Do not call docSpansOrdered(int,int,int,int) to avoid invoking .end() : */
			return (start1 == start2)?(spans1.End() < spans2.End()):(start1 < start2);
		}
		
		/// <summary>Like {@link #DocSpansOrdered(Spans,Spans)}, but use the spans
		/// starts and ends as parameters.
		/// </summary>
		private static bool DocSpansOrdered(int start1, int end1, int start2, int end2)
		{
			return (start1 == start2)?(end1 < end2):(start1 < start2);
		}
		
		/// <summary>Order the subSpans within the same document by advancing all later spans
		/// after the previous one.
		/// </summary>
		private bool StretchToOrder()
		{
			matchDoc = subSpans[0].Doc();
			for (int i = 1; inSameDoc && (i < subSpans.Length); i++)
			{
				while (!DocSpansOrdered(subSpans[i - 1], subSpans[i]))
				{
					if (!subSpans[i].Next())
					{
						inSameDoc = false;
						more = false;
						break;
					}
					else if (matchDoc != subSpans[i].Doc())
					{
						inSameDoc = false;
						break;
					}
				}
			}
			return inSameDoc;
		}
		
		/// <summary>The subSpans are ordered in the same doc, so there is a possible match.
		/// Compute the slop while making the match as short as possible by advancing
		/// all subSpans except the last one in reverse order.
		/// </summary>
		private bool ShrinkToAfterShortestMatch()
		{
			matchStart = subSpans[subSpans.Length - 1].Start();
			matchEnd = subSpans[subSpans.Length - 1].End();
            System.Collections.Generic.Dictionary<byte[], byte[]> possibleMatchPayloads = new System.Collections.Generic.Dictionary<byte[], byte[]>();
			if (subSpans[subSpans.Length - 1].IsPayloadAvailable())
			{
                System.Collections.Generic.ICollection<byte[]> payload = subSpans[subSpans.Length - 1].GetPayload();
                foreach(byte[] pl in payload)
                {
                    if (!possibleMatchPayloads.ContainsKey(pl))
                    {
                        possibleMatchPayloads.Add(pl, pl);
                    }
                }
			}
			
			System.Collections.Generic.List<byte[]> possiblePayload = null;
			
			int matchSlop = 0;
			int lastStart = matchStart;
			int lastEnd = matchEnd;
			for (int i = subSpans.Length - 2; i >= 0; i--)
			{
				Spans prevSpans = subSpans[i];
				if (collectPayloads && prevSpans.IsPayloadAvailable())
				{
					System.Collections.Generic.ICollection<byte[]> payload = prevSpans.GetPayload();
					possiblePayload = new System.Collections.Generic.List<byte[]>(payload.Count);
					possiblePayload.AddRange(payload);
				}
				
				int prevStart = prevSpans.Start();
				int prevEnd = prevSpans.End();
				while (true)
				{
					// Advance prevSpans until after (lastStart, lastEnd)
					if (!prevSpans.Next())
					{
						inSameDoc = false;
						more = false;
						break; // Check remaining subSpans for final match.
					}
					else if (matchDoc != prevSpans.Doc())
					{
						inSameDoc = false; // The last subSpans is not advanced here.
						break; // Check remaining subSpans for last match in this document.
					}
					else
					{
						int ppStart = prevSpans.Start();
						int ppEnd = prevSpans.End(); // Cannot avoid invoking .end()
						if (!DocSpansOrdered(ppStart, ppEnd, lastStart, lastEnd))
						{
							break; // Check remaining subSpans.
						}
						else
						{
							// prevSpans still before (lastStart, lastEnd)
							prevStart = ppStart;
							prevEnd = ppEnd;
							if (collectPayloads && prevSpans.IsPayloadAvailable())
							{
								System.Collections.Generic.ICollection<byte[]> payload = prevSpans.GetPayload();
								possiblePayload = new System.Collections.Generic.List<byte[]>(payload.Count);
								possiblePayload.AddRange(payload);
							}
						}
					}
				}
				
				if (collectPayloads && possiblePayload != null)
				{
                    foreach (byte[] pl in possiblePayload)
                    {
                        if (!possibleMatchPayloads.ContainsKey(pl))
                        {
                            possibleMatchPayloads.Add(pl, pl);
                        }
                    }
				}
				
				System.Diagnostics.Debug.Assert(prevStart <= matchStart);
				if (matchStart > prevEnd)
				{
					// Only non overlapping spans add to slop.
					matchSlop += (matchStart - prevEnd);
				}
				
				/* Do not break on (matchSlop > allowedSlop) here to make sure
				* that subSpans[0] is advanced after the match, if any.
				*/
				matchStart = prevStart;
				lastStart = prevStart;
				lastEnd = prevEnd;
			}
			
			bool match = matchSlop <= allowedSlop;
			
			if (collectPayloads && match && possibleMatchPayloads.Count > 0)
			{
				matchPayload.AddRange(possibleMatchPayloads.Keys);
			}
			
			return match; // ordered and allowed slop
		}
		
		public override System.String ToString()
		{
			return GetType().FullName + "(" + query.ToString() + ")@" + (firstTime?"START":(more?(Doc() + ":" + Start() + "-" + End()):"END"));
		}
	}
}
