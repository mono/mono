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

namespace Mono.Lucene.Net.Search
{
	
	/* Description from Doug Cutting (excerpted from
	* LUCENE-1483):
	*
	* BooleanScorer uses a ~16k array to score windows of
	* docs. So it scores docs 0-16k first, then docs 16-32k,
	* etc. For each window it iterates through all query terms
	* and accumulates a score in table[doc%16k]. It also stores
	* in the table a bitmask representing which terms
	* contributed to the score. Non-zero scores are chained in
	* a linked list. At the end of scoring each window it then
	* iterates through the linked list and, if the bitmask
	* matches the boolean constraints, collects a hit. For
	* boolean queries with lots of frequent terms this can be
	* much faster, since it does not need to update a priority
	* queue for each posting, instead performing constant-time
	* operations per posting. The only downside is that it
	* results in hits being delivered out-of-order within the
	* window, which means it cannot be nested within other
	* scorers. But it works well as a top-level scorer.
	*
	* The new BooleanScorer2 implementation instead works by
	* merging priority queues of postings, albeit with some
	* clever tricks. For example, a pure conjunction (all terms
	* required) does not require a priority queue. Instead it
	* sorts the posting streams at the start, then repeatedly
	* skips the first to to the last. If the first ever equals
	* the last, then there's a hit. When some terms are
	* required and some terms are optional, the conjunction can
	* be evaluated first, then the optional terms can all skip
	* to the match and be added to the score. Thus the
	* conjunction can reduce the number of priority queue
	* updates for the optional terms. */
	
	public sealed class BooleanScorer:Scorer
	{
		private void  InitBlock()
		{
			bucketTable = new BucketTable();
		}
		
		private sealed class BooleanScorerCollector:Collector
		{
			private BucketTable bucketTable;
			private int mask;
			private Scorer scorer;
			
			public BooleanScorerCollector(int mask, BucketTable bucketTable)
			{
				this.mask = mask;
				this.bucketTable = bucketTable;
			}
			public override void  Collect(int doc)
			{
				BucketTable table = bucketTable;
				int i = doc & Mono.Lucene.Net.Search.BooleanScorer.BucketTable.MASK;
				Bucket bucket = table.buckets[i];
				if (bucket == null)
					table.buckets[i] = bucket = new Bucket();
				
				if (bucket.doc != doc)
				{
					// invalid bucket
					bucket.doc = doc; // set doc
					bucket.score = scorer.Score(); // initialize score
					bucket.bits = mask; // initialize mask
					bucket.coord = 1; // initialize coord
					
					bucket.next = table.first; // push onto valid list
					table.first = bucket;
				}
				else
				{
					// valid bucket
					bucket.score += scorer.Score(); // increment score
					bucket.bits |= mask; // add bits in mask
					bucket.coord++; // increment coord
				}
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				// not needed by this implementation
			}
			
			public override void  SetScorer(Scorer scorer)
			{
				this.scorer = scorer;
			}
			
			public override bool AcceptsDocsOutOfOrder()
			{
				return true;
			}
		}
		
		// An internal class which is used in score(Collector, int) for setting the
		// current score. This is required since Collector exposes a setScorer method
		// and implementations that need the score will call scorer.score().
		// Therefore the only methods that are implemented are score() and doc().
		private sealed class BucketScorer:Scorer
		{
			
			internal float score;
			internal int doc = NO_MORE_DOCS;
			
			public BucketScorer():base(null)
			{
			}
			
			public override int Advance(int target)
			{
				return NO_MORE_DOCS;
			}
			
			/// <deprecated> use {@link #DocID()} instead. 
			/// </deprecated>
            [Obsolete("use DocID() instead.")]
			public override int Doc()
			{
				return doc;
			}
			
			public override int DocID()
			{
				return doc;
			}
			
			public override Explanation Explain(int doc)
			{
				return null;
			}
			
			/// <deprecated> use {@link #NextDoc()} instead. 
			/// </deprecated>
            [Obsolete("use NextDoc() instead. ")]
			public override bool Next()
			{
				return false;
			}
			
			public override int NextDoc()
			{
				return NO_MORE_DOCS;
			}
			
			public override float Score()
			{
				return score;
			}
			
			/// <deprecated> use {@link #Advance(int)} instead. 
			/// </deprecated>
            [Obsolete("use Advance(int) instead. ")]
			public override bool SkipTo(int target)
			{
				return false;
			}
		}
		
		internal sealed class Bucket
		{
			internal int doc = - 1; // tells if bucket is valid
			internal float score; // incremental score
			internal int bits; // used for bool constraints
			internal int coord; // count of terms in score
			internal Bucket next; // next valid bucket
		}
		
		/// <summary>A simple hash table of document scores within a range. </summary>
		internal sealed class BucketTable
		{
			private void  InitBlock()
			{
				buckets = new Bucket[SIZE];
			}
			public const int SIZE = 1 << 11;
			public static readonly int MASK;
			
			internal Bucket[] buckets;
			internal Bucket first = null; // head of valid list
			
			public BucketTable()
			{
                InitBlock();
			}
			
			public Collector NewCollector(int mask)
			{
				return new BooleanScorerCollector(mask, this);
			}
			
			public int Size()
			{
				return SIZE;
			}
			static BucketTable()
			{
				MASK = SIZE - 1;
			}
		}
		
		internal sealed class SubScorer
		{
			public Scorer scorer;
			public bool required = false;
			public bool prohibited = false;
			public Collector collector;
			public SubScorer next;
			
			public SubScorer(Scorer scorer, bool required, bool prohibited, Collector collector, SubScorer next)
			{
				this.scorer = scorer;
				this.required = required;
				this.prohibited = prohibited;
				this.collector = collector;
				this.next = next;
			}
		}
		
		private SubScorer scorers = null;
		private BucketTable bucketTable;
		private int maxCoord = 1;
		private float[] coordFactors;
		private int requiredMask = 0;
		private int prohibitedMask = 0;
		private int nextMask = 1;
		private int minNrShouldMatch;
		private int end;
		private Bucket current;
		private int doc = - 1;
		
		public /*internal*/ BooleanScorer(Similarity similarity, int minNrShouldMatch, System.Collections.IList optionalScorers, System.Collections.IList prohibitedScorers):base(similarity)
		{
			InitBlock();
			this.minNrShouldMatch = minNrShouldMatch;
			
			if (optionalScorers != null && optionalScorers.Count > 0)
			{
				for (System.Collections.IEnumerator si = optionalScorers.GetEnumerator(); si.MoveNext(); )
				{
					Scorer scorer = (Scorer) si.Current;
					maxCoord++;
					if (scorer.NextDoc() != NO_MORE_DOCS)
					{
						scorers = new SubScorer(scorer, false, false, bucketTable.NewCollector(0), scorers);
					}
				}
			}
			
			if (prohibitedScorers != null && prohibitedScorers.Count > 0)
			{
				for (System.Collections.IEnumerator si = prohibitedScorers.GetEnumerator(); si.MoveNext(); )
				{
					Scorer scorer = (Scorer) si.Current;
					int mask = nextMask;
					nextMask = nextMask << 1;
					prohibitedMask |= mask; // update prohibited mask
					if (scorer.NextDoc() != NO_MORE_DOCS)
					{
						scorers = new SubScorer(scorer, false, true, bucketTable.NewCollector(mask), scorers);
					}
				}
			}
			
			coordFactors = new float[maxCoord];
			Similarity sim = GetSimilarity();
			for (int i = 0; i < maxCoord; i++)
			{
				coordFactors[i] = sim.Coord(i, maxCoord - 1);
			}
		}
		
		// firstDocID is ignored since nextDoc() initializes 'current'
		public /*protected internal*/ override bool Score(Collector collector, int max, int firstDocID)
		{
			bool more;
			Bucket tmp;
			BucketScorer bs = new BucketScorer();
			// The internal loop will set the score and doc before calling collect.
			collector.SetScorer(bs);
			do 
			{
				bucketTable.first = null;
				
				while (current != null)
				{
					// more queued 
					
					// check prohibited & required
					if ((current.bits & prohibitedMask) == 0 && (current.bits & requiredMask) == requiredMask)
					{
						
						if (current.doc >= max)
						{
							tmp = current;
							current = current.next;
							tmp.next = bucketTable.first;
							bucketTable.first = tmp;
							continue;
						}
						
						if (current.coord >= minNrShouldMatch)
						{
							bs.score = current.score * coordFactors[current.coord];
							bs.doc = current.doc;
							collector.Collect(current.doc);
						}
					}
					
					current = current.next; // pop the queue
				}
				
				if (bucketTable.first != null)
				{
					current = bucketTable.first;
					bucketTable.first = current.next;
					return true;
				}
				
				// refill the queue
				more = false;
				end += BucketTable.SIZE;
				for (SubScorer sub = scorers; sub != null; sub = sub.next)
				{
					int subScorerDocID = sub.scorer.DocID();
					if (subScorerDocID != NO_MORE_DOCS)
					{
						more |= sub.scorer.Score(sub.collector, end, subScorerDocID);
					}
				}
				current = bucketTable.first;
			}
			while (current != null || more);
			
			return false;
		}
		
		/// <deprecated> use {@link #Score(Collector, int, int)} instead. 
		/// </deprecated>
        [Obsolete("use Score(Collector, int, int) instead.")]
		protected internal override bool Score(HitCollector hc, int max)
		{
			return Score(new HitCollectorWrapper(hc), max, DocID());
		}
		
		public override int Advance(int target)
		{
			throw new System.NotSupportedException();
		}
		
		/// <deprecated> use {@link #DocID()} instead. 
		/// </deprecated>
        [Obsolete("use DocID() instead. ")]
		public override int Doc()
		{
			return current.doc;
		}
		
		public override int DocID()
		{
			return doc;
		}
		
		public override Explanation Explain(int doc)
		{
			throw new System.NotSupportedException();
		}
		
		/// <deprecated> use {@link #NextDoc()} instead. 
		/// </deprecated>
        [Obsolete("use NextDoc() instead. ")]
		public override bool Next()
		{
			return NextDoc() != NO_MORE_DOCS;
		}
		
		public override int NextDoc()
		{
			bool more;
			do 
			{
				while (bucketTable.first != null)
				{
					// more queued
					current = bucketTable.first;
					bucketTable.first = current.next; // pop the queue
					
					// check prohibited & required, and minNrShouldMatch
					if ((current.bits & prohibitedMask) == 0 && (current.bits & requiredMask) == requiredMask && current.coord >= minNrShouldMatch)
					{
						return doc = current.doc;
					}
				}
				
				// refill the queue
				more = false;
				end += BucketTable.SIZE;
				for (SubScorer sub = scorers; sub != null; sub = sub.next)
				{
					Scorer scorer = sub.scorer;
					sub.collector.SetScorer(scorer);
					int doc = scorer.DocID();
					while (doc < end)
					{
						sub.collector.Collect(doc);
						doc = scorer.NextDoc();
					}
					more |= (doc != NO_MORE_DOCS);
				}
			}
			while (bucketTable.first != null || more);
			
			return this.doc = NO_MORE_DOCS;
		}
		
		public override float Score()
		{
			return current.score * coordFactors[current.coord];
		}
		
		public override void  Score(Collector collector)
		{
			Score(collector, System.Int32.MaxValue, NextDoc());
		}
		
		/// <deprecated> use {@link #Score(Collector)} instead. 
		/// </deprecated>
        [Obsolete("use Score(Collector) instead. ")]
		public override void  Score(HitCollector hc)
		{
			Score(new HitCollectorWrapper(hc));
		}
		
		/// <deprecated> use {@link #Advance(int)} instead. 
		/// </deprecated>
        [Obsolete("use Advance(int) instead. ")]
		public override bool SkipTo(int target)
		{
			throw new System.NotSupportedException();
		}
		
		public override System.String ToString()
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("boolean(");
			for (SubScorer sub = scorers; sub != null; sub = sub.next)
			{
				buffer.Append(sub.scorer.ToString());
				buffer.Append(" ");
			}
			buffer.Append(")");
			return buffer.ToString();
		}
	}
}
