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

using PriorityQueue = Mono.Lucene.Net.Util.PriorityQueue;

namespace Mono.Lucene.Net.Search
{
	
	public sealed class HitQueue:PriorityQueue
	{
		
		private bool prePopulate;
		
		/// <summary> Creates a new instance with <code>size</code> elements. If
		/// <code>prePopulate</code> is set to true, the queue will pre-populate itself
		/// with sentinel objects and set its {@link #Size()} to <code>size</code>. In
		/// that case, you should not rely on {@link #Size()} to get the number of
		/// actual elements that were added to the queue, but keep track yourself.<br/>
		/// <b>NOTE:</b> in case <code>prePopulate</code> is true, you should pop
		/// elements from the queue using the following code example:
		/// 
		/// <pre>
		/// PriorityQueue pq = new HitQueue(10, true); // pre-populate.
		/// ScoreDoc top = pq.top();
		/// 
		/// // Add/Update one element.
		/// top.score = 1.0f;
		/// top.doc = 0;
		/// top = (ScoreDoc) pq.updateTop();
		/// int totalHits = 1;
		/// 
		/// // Now pop only the elements that were *truly* inserted.
		/// // First, pop all the sentinel elements (there are pq.size() - totalHits).
		/// for (int i = pq.size() - totalHits; i &gt; 0; i--) pq.pop();
		/// 
		/// // Now pop the truly added elements.
		/// ScoreDoc[] results = new ScoreDoc[totalHits];
		/// for (int i = totalHits - 1; i &gt;= 0; i--) {
		/// results[i] = (ScoreDoc) pq.pop();
		/// }
		/// </pre>
		/// 
		/// <p/><b>NOTE</b>: This class pre-allocate a full array of
		/// length <code>size</code>.
		/// 
		/// </summary>
		/// <param name="size">the requested size of this queue.
		/// </param>
		/// <param name="prePopulate">specifies whether to pre-populate the queue with sentinel values.
		/// </param>
		/// <seealso cref="GetSentinelObject()">
		/// </seealso>
		public /*internal*/ HitQueue(int size, bool prePopulate)
		{
			this.prePopulate = prePopulate;
			Initialize(size);
		}
		
		// Returns null if prePopulate is false.
		protected internal override System.Object GetSentinelObject()
		{
			// Always set the doc Id to MAX_VALUE so that it won't be favored by
			// lessThan. This generally should not happen since if score is not NEG_INF,
			// TopScoreDocCollector will always add the object to the queue.
			return !prePopulate?null:new ScoreDoc(System.Int32.MaxValue, System.Single.NegativeInfinity);
		}
		
		public override bool LessThan(System.Object a, System.Object b)
		{
			ScoreDoc hitA = (ScoreDoc) a;
			ScoreDoc hitB = (ScoreDoc) b;
			if (hitA.score == hitB.score)
				return hitA.doc > hitB.doc;
			else
				return hitA.score < hitB.score;
		}
	}
}
