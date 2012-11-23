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
	
	/// <summary> Expert: Calculate query weights and build query scorers.
	/// <p/>
	/// The purpose of {@link Weight} is to ensure searching does not
	/// modify a {@link Query}, so that a {@link Query} instance can be reused. <br/>
	/// {@link Searcher} dependent state of the query should reside in the
	/// {@link Weight}. <br/>
	/// {@link IndexReader} dependent state should reside in the {@link Scorer}.
	/// <p/>
	/// A <code>Weight</code> is used in the following way:
	/// <ol>
	/// <li>A <code>Weight</code> is constructed by a top-level query, given a
	/// <code>Searcher</code> ({@link Query#CreateWeight(Searcher)}).</li>
	/// <li>The {@link #SumOfSquaredWeights()} method is called on the
	/// <code>Weight</code> to compute the query normalization factor
	/// {@link Similarity#QueryNorm(float)} of the query clauses contained in the
	/// query.</li>
	/// <li>The query normalization factor is passed to {@link #Normalize(float)}. At
	/// this point the weighting is complete.</li>
	/// <li>A <code>Scorer</code> is constructed by {@link #Scorer(IndexReader,boolean,boolean)}.</li>
	/// </ol>
	/// 
	/// </summary>
	/// <since> 2.9
	/// </since>
	[Serializable]
	public abstract class Weight
	{
		
		/// <summary> An explanation of the score computation for the named document.
		/// 
		/// </summary>
		/// <param name="reader">sub-reader containing the give doc
		/// </param>
		/// <param name="doc">
		/// </param>
		/// <returns> an Explanation for the score
		/// </returns>
		/// <throws>  IOException </throws>
		public abstract Explanation Explain(IndexReader reader, int doc);
		
		/// <summary>The query that this concerns. </summary>
		public abstract Query GetQuery();
		
		/// <summary>The weight for this query. </summary>
		public abstract float GetValue();
		
		/// <summary>Assigns the query normalization factor to this. </summary>
		public abstract void  Normalize(float norm);
		
		/// <summary> Returns a {@link Scorer} which scores documents in/out-of order according
		/// to <code>scoreDocsInOrder</code>.
		/// <p/>
		/// <b>NOTE:</b> even if <code>scoreDocsInOrder</code> is false, it is
		/// recommended to check whether the returned <code>Scorer</code> indeed scores
		/// documents out of order (i.e., call {@link #ScoresDocsOutOfOrder()}), as
		/// some <code>Scorer</code> implementations will always return documents
		/// in-order.<br/>
		/// <b>NOTE:</b> null can be returned if no documents will be scored by this
		/// query.
		/// 
		/// </summary>
		/// <param name="reader">
		/// the {@link IndexReader} for which to return the {@link Scorer}.
		/// </param>
		/// <param name="scoreDocsInOrder">specifies whether in-order scoring of documents is required. Note
		/// that if set to false (i.e., out-of-order scoring is required),
		/// this method can return whatever scoring mode it supports, as every
		/// in-order scorer is also an out-of-order one. However, an
		/// out-of-order scorer may not support {@link Scorer#NextDoc()}
		/// and/or {@link Scorer#Advance(int)}, therefore it is recommended to
		/// request an in-order scorer if use of these methods is required.
		/// </param>
		/// <param name="topScorer">
		/// if true, {@link Scorer#Score(Collector)} will be called; if false,
		/// {@link Scorer#NextDoc()} and/or {@link Scorer#Advance(int)} will
		/// be called.
		/// </param>
		/// <returns> a {@link Scorer} which scores documents in/out-of order.
		/// </returns>
		/// <throws>  IOException </throws>
		public abstract Scorer Scorer(IndexReader reader, bool scoreDocsInOrder, bool topScorer);
		
		/// <summary>The sum of squared weights of contained query clauses. </summary>
		public abstract float SumOfSquaredWeights();
		
		/// <summary> Returns true iff this implementation scores docs only out of order. This
		/// method is used in conjunction with {@link Collector}'s
		/// {@link Collector#AcceptsDocsOutOfOrder() acceptsDocsOutOfOrder} and
		/// {@link #Scorer(Mono.Lucene.Net.Index.IndexReader, boolean, boolean)} to
		/// create a matching {@link Scorer} instance for a given {@link Collector}, or
		/// vice versa.
		/// <p/>
		/// <b>NOTE:</b> the default implementation returns <code>false</code>, i.e.
		/// the <code>Scorer</code> scores documents in-order.
		/// </summary>
		public virtual bool ScoresDocsOutOfOrder()
		{
			return false;
		}
	}
}
