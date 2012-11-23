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
using PriorityQueue = Mono.Lucene.Net.Util.PriorityQueue;
using Entry = Mono.Lucene.Net.Search.FieldValueHitQueue.Entry;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A {@link Collector} that sorts by {@link SortField} using
	/// {@link FieldComparator}s.
	/// <p/>
	/// See the {@link #create(Mono.Lucene.Net.Search.Sort, int, boolean, boolean, boolean, boolean)} method
	/// for instantiating a TopFieldCollector.
	/// 
	/// <p/><b>NOTE:</b> This API is experimental and might change in
	/// incompatible ways in the next release.<p/>
	/// </summary>
	public abstract class TopFieldCollector:TopDocsCollector
	{
		
		// TODO: one optimization we could do is to pre-fill
		// the queue with sentinel value that guaranteed to
		// always compare lower than a real hit; this would
		// save having to check queueFull on each insert
		
		/*
		* Implements a TopFieldCollector over one SortField criteria, without
		* tracking document scores and maxScore.
		*/
		private class OneComparatorNonScoringCollector:TopFieldCollector
		{
			
			internal FieldComparator comparator;
			internal int reverseMul;
			
			public OneComparatorNonScoringCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
				comparator = queue.GetComparators()[0];
				reverseMul = queue.GetReverseMul()[0];
			}
			
			internal void  UpdateBottom(int doc)
			{
				// bottom.score is already set to Float.NaN in add().
				bottom.docID = docBase + doc;
				bottom = (Entry) pq.UpdateTop();
			}
			
			public override void  Collect(int doc)
			{
				++totalHits;
				if (queueFull)
				{
					if ((reverseMul * comparator.CompareBottom(doc)) <= 0)
					{
						// since docs are visited in doc Id order, if compare is 0, it means
						// this document is largest than anything else in the queue, and
						// therefore not competitive.
						return ;
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					comparator.Copy(bottom.slot, doc);
					UpdateBottom(doc);
					comparator.SetBottom(bottom.slot);
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					comparator.Copy(slot, doc);
					Add(slot, doc, System.Single.NaN);
					if (queueFull)
					{
						comparator.SetBottom(bottom.slot);
					}
				}
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				this.docBase = docBase;
				comparator.SetNextReader(reader, docBase);
			}
			
			public override void  SetScorer(Scorer scorer)
			{
				comparator.SetScorer(scorer);
			}
		}
		
		/*
		* Implements a TopFieldCollector over one SortField criteria, without
		* tracking document scores and maxScore, and assumes out of orderness in doc
		* Ids collection.
		*/
		private class OutOfOrderOneComparatorNonScoringCollector:OneComparatorNonScoringCollector
		{
			
			public OutOfOrderOneComparatorNonScoringCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
			}
			
			public override void  Collect(int doc)
			{
				++totalHits;
				if (queueFull)
				{
					// Fastmatch: return if this hit is not competitive
					int cmp = reverseMul * comparator.CompareBottom(doc);
					if (cmp < 0 || (cmp == 0 && doc + docBase > bottom.docID))
					{
						return ;
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					comparator.Copy(bottom.slot, doc);
					UpdateBottom(doc);
					comparator.SetBottom(bottom.slot);
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					comparator.Copy(slot, doc);
					Add(slot, doc, System.Single.NaN);
					if (queueFull)
					{
						comparator.SetBottom(bottom.slot);
					}
				}
			}
			
			public override bool AcceptsDocsOutOfOrder()
			{
				return true;
			}
		}
		
		/*
		* Implements a TopFieldCollector over one SortField criteria, while tracking
		* document scores but no maxScore.
		*/
		private class OneComparatorScoringNoMaxScoreCollector:OneComparatorNonScoringCollector
		{
			
			internal Scorer scorer;
			
			public OneComparatorScoringNoMaxScoreCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
			}
			
			internal void  updateBottom(int doc, float score)
			{
				bottom.docID = docBase + doc;
				bottom.score = score;
				bottom = (Entry) pq.UpdateTop();
			}
			
			public override void  Collect(int doc)
			{
				++totalHits;
				if (queueFull)
				{
					if ((reverseMul * comparator.CompareBottom(doc)) <= 0)
					{
						// since docs are visited in doc Id order, if compare is 0, it means
						// this document is largest than anything else in the queue, and
						// therefore not competitive.
						return ;
					}
					
					// Compute the score only if the hit is competitive.
					float score = scorer.Score();
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					comparator.Copy(bottom.slot, doc);
					updateBottom(doc, score);
					comparator.SetBottom(bottom.slot);
				}
				else
				{
					// Compute the score only if the hit is competitive.
					float score = scorer.Score();
					
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					comparator.Copy(slot, doc);
					Add(slot, doc, score);
					if (queueFull)
					{
						comparator.SetBottom(bottom.slot);
					}
				}
			}
			
			public override void  SetScorer(Scorer scorer)
			{
				this.scorer = scorer;
				comparator.SetScorer(scorer);
			}
		}
		
		/*
		* Implements a TopFieldCollector over one SortField criteria, while tracking
		* document scores but no maxScore, and assumes out of orderness in doc Ids
		* collection.
		*/
		private class OutOfOrderOneComparatorScoringNoMaxScoreCollector:OneComparatorScoringNoMaxScoreCollector
		{
			
			public OutOfOrderOneComparatorScoringNoMaxScoreCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
			}
			
			public override void  Collect(int doc)
			{
				++totalHits;
				if (queueFull)
				{
					// Fastmatch: return if this hit is not competitive
					int cmp = reverseMul * comparator.CompareBottom(doc);
					if (cmp < 0 || (cmp == 0 && doc + docBase > bottom.docID))
					{
						return ;
					}
					
					// Compute the score only if the hit is competitive.
					float score = scorer.Score();
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					comparator.Copy(bottom.slot, doc);
					updateBottom(doc, score);
					comparator.SetBottom(bottom.slot);
				}
				else
				{
					// Compute the score only if the hit is competitive.
					float score = scorer.Score();
					
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					comparator.Copy(slot, doc);
					Add(slot, doc, score);
					if (queueFull)
					{
						comparator.SetBottom(bottom.slot);
					}
				}
			}
			
			public override bool AcceptsDocsOutOfOrder()
			{
				return true;
			}
		}
		
		/*
		* Implements a TopFieldCollector over one SortField criteria, with tracking
		* document scores and maxScore.
		*/
		private class OneComparatorScoringMaxScoreCollector:OneComparatorNonScoringCollector
		{
			
			internal Scorer scorer;
			
			public OneComparatorScoringMaxScoreCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
				// Must set maxScore to NEG_INF, or otherwise Math.max always returns NaN.
				maxScore = System.Single.NegativeInfinity;
			}
			
			internal void  UpdateBottom(int doc, float score)
			{
				bottom.docID = docBase + doc;
				bottom.score = score;
				bottom = (Entry) pq.UpdateTop();
			}
			
			public override void  Collect(int doc)
			{
				float score = scorer.Score();
				if (score > maxScore)
				{
					maxScore = score;
				}
				++totalHits;
				if (queueFull)
				{
					if ((reverseMul * comparator.CompareBottom(doc)) <= 0)
					{
						// since docs are visited in doc Id order, if compare is 0, it means
						// this document is largest than anything else in the queue, and
						// therefore not competitive.
						return ;
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					comparator.Copy(bottom.slot, doc);
					UpdateBottom(doc, score);
					comparator.SetBottom(bottom.slot);
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					comparator.Copy(slot, doc);
					Add(slot, doc, score);
					if (queueFull)
					{
						comparator.SetBottom(bottom.slot);
					}
				}
			}
			
			public override void  SetScorer(Scorer scorer)
			{
				this.scorer = scorer;
				base.SetScorer(scorer);
			}
		}
		
		/*
		* Implements a TopFieldCollector over one SortField criteria, with tracking
		* document scores and maxScore, and assumes out of orderness in doc Ids
		* collection.
		*/
		private class OutOfOrderOneComparatorScoringMaxScoreCollector:OneComparatorScoringMaxScoreCollector
		{
			
			public OutOfOrderOneComparatorScoringMaxScoreCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
			}
			
			public override void  Collect(int doc)
			{
				float score = scorer.Score();
				if (score > maxScore)
				{
					maxScore = score;
				}
				++totalHits;
				if (queueFull)
				{
					// Fastmatch: return if this hit is not competitive
					int cmp = reverseMul * comparator.CompareBottom(doc);
					if (cmp < 0 || (cmp == 0 && doc + docBase > bottom.docID))
					{
						return ;
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					comparator.Copy(bottom.slot, doc);
					UpdateBottom(doc, score);
					comparator.SetBottom(bottom.slot);
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					comparator.Copy(slot, doc);
					Add(slot, doc, score);
					if (queueFull)
					{
						comparator.SetBottom(bottom.slot);
					}
				}
			}
			
			public override bool AcceptsDocsOutOfOrder()
			{
				return true;
			}
		}
		
		/*
		* Implements a TopFieldCollector over multiple SortField criteria, without
		* tracking document scores and maxScore.
		*/
		private class MultiComparatorNonScoringCollector:TopFieldCollector
		{
			
			internal FieldComparator[] comparators;
			internal int[] reverseMul;
			
			public MultiComparatorNonScoringCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
				comparators = queue.GetComparators();
				reverseMul = queue.GetReverseMul();
			}
			
			internal void  UpdateBottom(int doc)
			{
				// bottom.score is already set to Float.NaN in add().
				bottom.docID = docBase + doc;
				bottom = (Entry) pq.UpdateTop();
			}
			
			public override void  Collect(int doc)
			{
				++totalHits;
				if (queueFull)
				{
					// Fastmatch: return if this hit is not competitive
					for (int i = 0; ; i++)
					{
						int c = reverseMul[i] * comparators[i].CompareBottom(doc);
						if (c < 0)
						{
							// Definitely not competitive.
							return ;
						}
						else if (c > 0)
						{
							// Definitely competitive.
							break;
						}
						else if (i == comparators.Length - 1)
						{
							// Here c=0. If we're at the last comparator, this doc is not
							// competitive, since docs are visited in doc Id order, which means
							// this doc cannot compete with any other document in the queue.
							return ;
						}
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(bottom.slot, doc);
					}
					
					UpdateBottom(doc);
					
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].SetBottom(bottom.slot);
					}
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(slot, doc);
					}
					Add(slot, doc, System.Single.NaN);
					if (queueFull)
					{
						for (int i = 0; i < comparators.Length; i++)
						{
							comparators[i].SetBottom(bottom.slot);
						}
					}
				}
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				this.docBase = docBase;
				for (int i = 0; i < comparators.Length; i++)
				{
					comparators[i].SetNextReader(reader, docBase);
				}
			}
			
			public override void  SetScorer(Scorer scorer)
			{
				// set the scorer on all comparators
				for (int i = 0; i < comparators.Length; i++)
				{
					comparators[i].SetScorer(scorer);
				}
			}
		}
		
		/*
		* Implements a TopFieldCollector over multiple SortField criteria, without
		* tracking document scores and maxScore, and assumes out of orderness in doc
		* Ids collection.
		*/
		private class OutOfOrderMultiComparatorNonScoringCollector:MultiComparatorNonScoringCollector
		{
			
			public OutOfOrderMultiComparatorNonScoringCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
			}
			
			public override void  Collect(int doc)
			{
				++totalHits;
				if (queueFull)
				{
					// Fastmatch: return if this hit is not competitive
					for (int i = 0; ; i++)
					{
						int c = reverseMul[i] * comparators[i].CompareBottom(doc);
						if (c < 0)
						{
							// Definitely not competitive.
							return ;
						}
						else if (c > 0)
						{
							// Definitely competitive.
							break;
						}
						else if (i == comparators.Length - 1)
						{
							// This is the equals case.
							if (doc + docBase > bottom.docID)
							{
								// Definitely not competitive
								return ;
							}
							break;
						}
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(bottom.slot, doc);
					}
					
					UpdateBottom(doc);
					
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].SetBottom(bottom.slot);
					}
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(slot, doc);
					}
					Add(slot, doc, System.Single.NaN);
					if (queueFull)
					{
						for (int i = 0; i < comparators.Length; i++)
						{
							comparators[i].SetBottom(bottom.slot);
						}
					}
				}
			}
			
			public override bool AcceptsDocsOutOfOrder()
			{
				return true;
			}
		}
		
		/*
		* Implements a TopFieldCollector over multiple SortField criteria, with
		* tracking document scores and maxScore.
		*/
		private class MultiComparatorScoringMaxScoreCollector:MultiComparatorNonScoringCollector
		{
			
			internal Scorer scorer;
			
			public MultiComparatorScoringMaxScoreCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
				// Must set maxScore to NEG_INF, or otherwise Math.max always returns NaN.
				maxScore = System.Single.NegativeInfinity;
			}
			
			internal void  UpdateBottom(int doc, float score)
			{
				bottom.docID = docBase + doc;
				bottom.score = score;
				bottom = (Entry) pq.UpdateTop();
			}
			
			public override void  Collect(int doc)
			{
				float score = scorer.Score();
				if (score > maxScore)
				{
					maxScore = score;
				}
				++totalHits;
				if (queueFull)
				{
					// Fastmatch: return if this hit is not competitive
					for (int i = 0; ; i++)
					{
						int c = reverseMul[i] * comparators[i].CompareBottom(doc);
						if (c < 0)
						{
							// Definitely not competitive.
							return ;
						}
						else if (c > 0)
						{
							// Definitely competitive.
							break;
						}
						else if (i == comparators.Length - 1)
						{
							// Here c=0. If we're at the last comparator, this doc is not
							// competitive, since docs are visited in doc Id order, which means
							// this doc cannot compete with any other document in the queue.
							return ;
						}
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(bottom.slot, doc);
					}
					
					UpdateBottom(doc, score);
					
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].SetBottom(bottom.slot);
					}
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(slot, doc);
					}
					Add(slot, doc, score);
					if (queueFull)
					{
						for (int i = 0; i < comparators.Length; i++)
						{
							comparators[i].SetBottom(bottom.slot);
						}
					}
				}
			}
			
			public override void  SetScorer(Scorer scorer)
			{
				this.scorer = scorer;
				base.SetScorer(scorer);
			}
		}
		
		/*
		* Implements a TopFieldCollector over multiple SortField criteria, with
		* tracking document scores and maxScore, and assumes out of orderness in doc
		* Ids collection.
		*/
		private sealed class OutOfOrderMultiComparatorScoringMaxScoreCollector:MultiComparatorScoringMaxScoreCollector
		{
			
			public OutOfOrderMultiComparatorScoringMaxScoreCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
			}
			
			public override void  Collect(int doc)
			{
				float score = scorer.Score();
				if (score > maxScore)
				{
					maxScore = score;
				}
				++totalHits;
				if (queueFull)
				{
					// Fastmatch: return if this hit is not competitive
					for (int i = 0; ; i++)
					{
						int c = reverseMul[i] * comparators[i].CompareBottom(doc);
						if (c < 0)
						{
							// Definitely not competitive.
							return ;
						}
						else if (c > 0)
						{
							// Definitely competitive.
							break;
						}
						else if (i == comparators.Length - 1)
						{
							// This is the equals case.
							if (doc + docBase > bottom.docID)
							{
								// Definitely not competitive
								return ;
							}
							break;
						}
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(bottom.slot, doc);
					}
					
					UpdateBottom(doc, score);
					
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].SetBottom(bottom.slot);
					}
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(slot, doc);
					}
					Add(slot, doc, score);
					if (queueFull)
					{
						for (int i = 0; i < comparators.Length; i++)
						{
							comparators[i].SetBottom(bottom.slot);
						}
					}
				}
			}
			
			public override bool AcceptsDocsOutOfOrder()
			{
				return true;
			}
		}
		
		/*
		* Implements a TopFieldCollector over multiple SortField criteria, with
		* tracking document scores and maxScore.
		*/
		private class MultiComparatorScoringNoMaxScoreCollector:MultiComparatorNonScoringCollector
		{
			
			internal Scorer scorer;
			
			public MultiComparatorScoringNoMaxScoreCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
			}
			
			internal void  UpdateBottom(int doc, float score)
			{
				bottom.docID = docBase + doc;
				bottom.score = score;
				bottom = (Entry) pq.UpdateTop();
			}
			
			public override void  Collect(int doc)
			{
				++totalHits;
				if (queueFull)
				{
					// Fastmatch: return if this hit is not competitive
					for (int i = 0; ; i++)
					{
						int c = reverseMul[i] * comparators[i].CompareBottom(doc);
						if (c < 0)
						{
							// Definitely not competitive.
							return ;
						}
						else if (c > 0)
						{
							// Definitely competitive.
							break;
						}
						else if (i == comparators.Length - 1)
						{
							// Here c=0. If we're at the last comparator, this doc is not
							// competitive, since docs are visited in doc Id order, which means
							// this doc cannot compete with any other document in the queue.
							return ;
						}
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(bottom.slot, doc);
					}
					
					// Compute score only if it is competitive.
					float score = scorer.Score();
					UpdateBottom(doc, score);
					
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].SetBottom(bottom.slot);
					}
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(slot, doc);
					}
					
					// Compute score only if it is competitive.
					float score = scorer.Score();
					Add(slot, doc, score);
					if (queueFull)
					{
						for (int i = 0; i < comparators.Length; i++)
						{
							comparators[i].SetBottom(bottom.slot);
						}
					}
				}
			}
			
			public override void  SetScorer(Scorer scorer)
			{
				this.scorer = scorer;
				base.SetScorer(scorer);
			}
		}
		
		/*
		* Implements a TopFieldCollector over multiple SortField criteria, with
		* tracking document scores and maxScore, and assumes out of orderness in doc
		* Ids collection.
		*/
		private sealed class OutOfOrderMultiComparatorScoringNoMaxScoreCollector:MultiComparatorScoringNoMaxScoreCollector
		{
			
			public OutOfOrderMultiComparatorScoringNoMaxScoreCollector(FieldValueHitQueue queue, int numHits, bool fillFields):base(queue, numHits, fillFields)
			{
			}
			
			public override void  Collect(int doc)
			{
				++totalHits;
				if (queueFull)
				{
					// Fastmatch: return if this hit is not competitive
					for (int i = 0; ; i++)
					{
						int c = reverseMul[i] * comparators[i].CompareBottom(doc);
						if (c < 0)
						{
							// Definitely not competitive.
							return ;
						}
						else if (c > 0)
						{
							// Definitely competitive.
							break;
						}
						else if (i == comparators.Length - 1)
						{
							// This is the equals case.
							if (doc + docBase > bottom.docID)
							{
								// Definitely not competitive
								return ;
							}
							break;
						}
					}
					
					// This hit is competitive - replace bottom element in queue & adjustTop
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(bottom.slot, doc);
					}
					
					// Compute score only if it is competitive.
					float score = scorer.Score();
					UpdateBottom(doc, score);
					
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].SetBottom(bottom.slot);
					}
				}
				else
				{
					// Startup transient: queue hasn't gathered numHits yet
					int slot = totalHits - 1;
					// Copy hit into queue
					for (int i = 0; i < comparators.Length; i++)
					{
						comparators[i].Copy(slot, doc);
					}
					
					// Compute score only if it is competitive.
					float score = scorer.Score();
					Add(slot, doc, score);
					if (queueFull)
					{
						for (int i = 0; i < comparators.Length; i++)
						{
							comparators[i].SetBottom(bottom.slot);
						}
					}
				}
			}
			
			public override void  SetScorer(Scorer scorer)
			{
				this.scorer = scorer;
				base.SetScorer(scorer);
			}
			
			public override bool AcceptsDocsOutOfOrder()
			{
				return true;
			}
		}
		
		private static readonly ScoreDoc[] EMPTY_SCOREDOCS = new ScoreDoc[0];
		
		private bool fillFields;
		
		/*
		* Stores the maximum score value encountered, needed for normalizing. If
		* document scores are not tracked, this value is initialized to NaN.
		*/
		internal float maxScore = System.Single.NaN;
		
		internal int numHits;
		internal FieldValueHitQueue.Entry bottom = null;
		internal bool queueFull;
		internal int docBase;
		
		// Declaring the constructor private prevents extending this class by anyone
		// else. Note that the class cannot be final since it's extended by the
		// internal versions. If someone will define a constructor with any other
		// visibility, then anyone will be able to extend the class, which is not what
		// we want.
		private TopFieldCollector(PriorityQueue pq, int numHits, bool fillFields):base(pq)
		{
			this.numHits = numHits;
			this.fillFields = fillFields;
		}
		
		/// <summary> Creates a new {@link TopFieldCollector} from the given
		/// arguments.
		/// 
		/// <p/><b>NOTE</b>: The instances returned by this method
		/// pre-allocate a full array of length
		/// <code>numHits</code>.
		/// 
		/// </summary>
		/// <param name="sort">the sort criteria (SortFields).
		/// </param>
		/// <param name="numHits">the number of results to collect.
		/// </param>
		/// <param name="fillFields">specifies whether the actual field values should be returned on
		/// the results (FieldDoc).
		/// </param>
		/// <param name="trackDocScores">specifies whether document scores should be tracked and set on the
		/// results. Note that if set to false, then the results' scores will
		/// be set to Float.NaN. Setting this to true affects performance, as
		/// it incurs the score computation on each competitive result.
		/// Therefore if document scores are not required by the application,
		/// it is recommended to set it to false.
		/// </param>
		/// <param name="trackMaxScore">specifies whether the query's maxScore should be tracked and set
		/// on the resulting {@link TopDocs}. Note that if set to false,
		/// {@link TopDocs#GetMaxScore()} returns Float.NaN. Setting this to
		/// true affects performance as it incurs the score computation on
		/// each result. Also, setting this true automatically sets
		/// <code>trackDocScores</code> to true as well.
		/// </param>
		/// <param name="docsScoredInOrder">specifies whether documents are scored in doc Id order or not by
		/// the given {@link Scorer} in {@link #SetScorer(Scorer)}.
		/// </param>
		/// <returns> a {@link TopFieldCollector} instance which will sort the results by
		/// the sort criteria.
		/// </returns>
		/// <throws>  IOException </throws>
		public static TopFieldCollector create(Sort sort, int numHits, bool fillFields, bool trackDocScores, bool trackMaxScore, bool docsScoredInOrder)
		{
			if (sort.fields.Length == 0)
			{
				throw new System.ArgumentException("Sort must contain at least one field");
			}
			
			FieldValueHitQueue queue = FieldValueHitQueue.Create(sort.fields, numHits);
			if (queue.GetComparators().Length == 1)
			{
				if (docsScoredInOrder)
				{
					if (trackMaxScore)
					{
						return new OneComparatorScoringMaxScoreCollector(queue, numHits, fillFields);
					}
					else if (trackDocScores)
					{
						return new OneComparatorScoringNoMaxScoreCollector(queue, numHits, fillFields);
					}
					else
					{
						return new OneComparatorNonScoringCollector(queue, numHits, fillFields);
					}
				}
				else
				{
					if (trackMaxScore)
					{
						return new OutOfOrderOneComparatorScoringMaxScoreCollector(queue, numHits, fillFields);
					}
					else if (trackDocScores)
					{
						return new OutOfOrderOneComparatorScoringNoMaxScoreCollector(queue, numHits, fillFields);
					}
					else
					{
						return new OutOfOrderOneComparatorNonScoringCollector(queue, numHits, fillFields);
					}
				}
			}
			
			// multiple comparators.
			if (docsScoredInOrder)
			{
				if (trackMaxScore)
				{
					return new MultiComparatorScoringMaxScoreCollector(queue, numHits, fillFields);
				}
				else if (trackDocScores)
				{
					return new MultiComparatorScoringNoMaxScoreCollector(queue, numHits, fillFields);
				}
				else
				{
					return new MultiComparatorNonScoringCollector(queue, numHits, fillFields);
				}
			}
			else
			{
				if (trackMaxScore)
				{
					return new OutOfOrderMultiComparatorScoringMaxScoreCollector(queue, numHits, fillFields);
				}
				else if (trackDocScores)
				{
					return new OutOfOrderMultiComparatorScoringNoMaxScoreCollector(queue, numHits, fillFields);
				}
				else
				{
					return new OutOfOrderMultiComparatorNonScoringCollector(queue, numHits, fillFields);
				}
			}
		}
		
		internal void  Add(int slot, int doc, float score)
		{
			bottom = (Entry) pq.Add(new Entry(slot, docBase + doc, score));
			queueFull = totalHits == numHits;
		}
		
		/*
		* Only the following callback methods need to be overridden since
		* topDocs(int, int) calls them to return the results.
		*/
		
		protected internal override void  PopulateResults(ScoreDoc[] results, int howMany)
		{
			if (fillFields)
			{
				// avoid casting if unnecessary.
				FieldValueHitQueue queue = (FieldValueHitQueue) pq;
				for (int i = howMany - 1; i >= 0; i--)
				{
					results[i] = queue.FillFields((Entry) queue.Pop());
				}
			}
			else
			{
				for (int i = howMany - 1; i >= 0; i--)
				{
					Entry entry = (Entry) pq.Pop();
					results[i] = new FieldDoc(entry.docID, entry.score);
				}
			}
		}
		
		public /*protected internal*/ override TopDocs NewTopDocs(ScoreDoc[] results, int start)
		{
			if (results == null)
			{
				results = EMPTY_SCOREDOCS;
				// Set maxScore to NaN, in case this is a maxScore tracking collector.
				maxScore = System.Single.NaN;
			}
			
			// If this is a maxScoring tracking collector and there were no results, 
			return new TopFieldDocs(totalHits, results, ((FieldValueHitQueue) pq).GetFields(), maxScore);
		}
		
		public override bool AcceptsDocsOutOfOrder()
		{
			return false;
		}
	}
}
