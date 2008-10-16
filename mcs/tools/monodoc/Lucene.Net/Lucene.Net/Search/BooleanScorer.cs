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
namespace Monodoc.Lucene.Net.Search
{
	
	sealed class BooleanScorer : Scorer
	{
		private void  InitBlock()
		{
			bucketTable = new BucketTable(this);
		}
		private SubScorer scorers = null;
		private BucketTable bucketTable;
		
		private int maxCoord = 1;
		private float[] coordFactors = null;
		
		private int requiredMask = 0;
		private int prohibitedMask = 0;
		private int nextMask = 1;
		
		internal BooleanScorer(Similarity similarity) : base(similarity)
		{
			InitBlock();
		}
		
		internal sealed class SubScorer
		{
			public Scorer scorer;
			public bool done;
			public bool required = false;
			public bool prohibited = false;
			public HitCollector collector;
			public SubScorer next;
			
			public SubScorer(Scorer scorer, bool required, bool prohibited, HitCollector collector, SubScorer next)
			{
				this.scorer = scorer;
				this.done = !scorer.Next();
				this.required = required;
				this.prohibited = prohibited;
				this.collector = collector;
				this.next = next;
			}
		}
		
		internal void  Add(Scorer scorer, bool required, bool prohibited)
		{
			int mask = 0;
			if (required || prohibited)
			{
				if (nextMask == 0)
					throw new System.IndexOutOfRangeException("More than 32 required/prohibited clauses in query.");
				mask = nextMask;
				nextMask = nextMask << 1;
			}
			else
				mask = 0;
			
			if (!prohibited)
				maxCoord++;
			
			if (prohibited)
				prohibitedMask |= mask;
			// update prohibited mask
			else if (required)
				requiredMask |= mask; // update required mask
			
			scorers = new SubScorer(scorer, required, prohibited, bucketTable.NewCollector(mask), scorers);
		}
		
		private void  ComputeCoordFactors()
		{
			coordFactors = new float[maxCoord];
			for (int i = 0; i < maxCoord; i++)
				coordFactors[i] = GetSimilarity().Coord(i, maxCoord - 1);
		}
		
		private int end;
		private Bucket current;
		
		public override int Doc()
		{
			return current.doc;
		}
		
		public override bool Next()
		{
			bool more;
			do 
			{
				while (bucketTable.first != null)
				{
					// more queued
					current = bucketTable.first;
					bucketTable.first = current.next; // pop the queue
					
					// check prohibited & required
					if ((current.bits & prohibitedMask) == 0 && (current.bits & requiredMask) == requiredMask)
					{
						return true;
					}
                }
				
				// refill the queue
				more = false;
				end += BucketTable.SIZE;
				for (SubScorer sub = scorers; sub != null; sub = sub.next)
				{
					Scorer scorer = sub.scorer;
					while (!sub.done && scorer.Doc() < end)
					{
						sub.collector.Collect(scorer.Doc(), scorer.Score());
						sub.done = !scorer.Next();
					}
					if (!sub.done)
					{
						more = true;
					}
				}
			}
			while (bucketTable.first != null | more);
			
			return false;
		}
		
		public override float Score()
		{
			if (coordFactors == null)
				ComputeCoordFactors();
			return current.score * coordFactors[current.coord];
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
			public const int SIZE = 1 << 10;
			public static readonly int MASK;
			
			internal Bucket[] buckets;
			internal Bucket first = null; // head of valid list
			
			private BooleanScorer scorer;
			
			public BucketTable(BooleanScorer scorer)
			{
                InitBlock();
				this.scorer = scorer;
			}
			
			public int Size()
			{
				return SIZE;
			}
			
			public HitCollector NewCollector(int mask)
			{
				return new Collector(mask, this);
			}
			static BucketTable()
			{
				MASK = SIZE - 1;
			}
		}
		
		internal sealed class Collector : HitCollector
		{
			private BucketTable bucketTable;
			private int mask;
			public Collector(int mask, BucketTable bucketTable)
			{
				this.mask = mask;
				this.bucketTable = bucketTable;
			}
			public override void  Collect(int doc, float score)
			{
				BucketTable table = bucketTable;
				int i = doc & Monodoc.Lucene.Net.Search.BooleanScorer.BucketTable.MASK;
				Bucket bucket = table.buckets[i];
				if (bucket == null)
					table.buckets[i] = bucket = new Bucket();
				
				if (bucket.doc != doc)
				{
					// invalid bucket
					bucket.doc = doc; // set doc
					bucket.score = score; // initialize score
					bucket.bits = mask; // initialize mask
					bucket.coord = 1; // initialize coord
					
					bucket.next = table.first; // push onto valid list
					table.first = bucket;
				}
				else
				{
					// valid bucket
					bucket.score += score; // increment score
					bucket.bits |= mask; // add bits in mask
					bucket.coord++; // increment coord
				}
			}
		}
		
		public override bool SkipTo(int target)
		{
			throw new System.NotSupportedException();
		}
		
		public override Explanation Explain(int doc)
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