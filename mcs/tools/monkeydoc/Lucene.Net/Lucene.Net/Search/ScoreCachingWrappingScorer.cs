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

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A {@link Scorer} which wraps another scorer and caches the score of the
	/// current document. Successive calls to {@link #Score()} will return the same
	/// result and will not invoke the wrapped Scorer's score() method, unless the
	/// current document has changed.<br/>
	/// This class might be useful due to the changes done to the {@link Collector}
	/// interface, in which the score is not computed for a document by default, only
	/// if the collector requests it. Some collectors may need to use the score in
	/// several places, however all they have in hand is a {@link Scorer} object, and
	/// might end up computing the score of a document more than once.
	/// </summary>
	public class ScoreCachingWrappingScorer:Scorer
	{
		
		private Scorer scorer;
		private int curDoc = - 1;
		private float curScore;
		
		/// <summary>Creates a new instance by wrapping the given scorer. </summary>
		public ScoreCachingWrappingScorer(Scorer scorer):base(scorer.GetSimilarity())
		{
			this.scorer = scorer;
		}
		
		public /*protected internal*/ override bool Score(Collector collector, int max, int firstDocID)
		{
			return scorer.Score(collector, max, firstDocID);
		}
		
		public override Similarity GetSimilarity()
		{
			return scorer.GetSimilarity();
		}
		
		public override Explanation Explain(int doc)
		{
			return scorer.Explain(doc);
		}
		
		public override float Score()
		{
			int doc = scorer.DocID();
			if (doc != curDoc)
			{
				curScore = scorer.Score();
				curDoc = doc;
			}
			
			return curScore;
		}
		
		/// <deprecated> use {@link #DocID()} instead. 
		/// </deprecated>
        [Obsolete("use DocID() instead.")]
		public override int Doc()
		{
			return scorer.Doc();
		}
		
		public override int DocID()
		{
			return scorer.DocID();
		}
		
		/// <deprecated> use {@link #NextDoc()} instead. 
		/// </deprecated>
        [Obsolete("use NextDoc() instead.")]
		public override bool Next()
		{
			return scorer.Next();
		}
		
		public override int NextDoc()
		{
			return scorer.NextDoc();
		}
		
		public override void  Score(Collector collector)
		{
			scorer.Score(collector);
		}
		
		/// <deprecated> use {@link #Advance(int)} instead. 
		/// </deprecated>
        [Obsolete("use Advance(int) instead.")]
		public override bool SkipTo(int target)
		{
			return scorer.SkipTo(target);
		}
		
		public override int Advance(int target)
		{
			return scorer.Advance(target);
		}
	}
}
