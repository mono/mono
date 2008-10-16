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
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>Expert: Calculate query weights and build query scorers.
	/// 
	/// <p>A Weight is constructed by a query, given a Searcher ({@link
	/// Query#CreateWeight(Searcher)}).  The {@link #SumOfSquaredWeights()} method
	/// is then called on the top-level query to compute the query normalization
	/// factor (@link Similarity#queryNorm(float)}).  This factor is then passed to
	/// {@link #Normalize(float)}.  At this point the weighting is complete and a
	/// scorer may be constructed by calling {@link #Scorer(Monodoc.Lucene.Net.Index.IndexReader)}.
	/// </summary>
	public interface Weight
	{
        /// <summary>The query that this concerns. </summary>
        Query Query
        {
            get;
			
        }
        /// <summary>The weight for this query. </summary>
        float Value
        {
            get;
			
        }
		
		/// <summary>The sum of squared weights of contained query clauses. </summary>
		float SumOfSquaredWeights();
		
		/// <summary>Assigns the query normalization factor to this. </summary>
		void  Normalize(float norm);
		
		/// <summary>Constructs a scorer for this. </summary>
		Scorer Scorer(Monodoc.Lucene.Net.Index.IndexReader reader);
		
		/// <summary>An explanation of the score computation for the named document. </summary>
		Explanation Explain(Monodoc.Lucene.Net.Index.IndexReader reader, int doc);
	}
}