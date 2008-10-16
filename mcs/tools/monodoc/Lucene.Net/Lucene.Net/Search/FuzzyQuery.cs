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
using Term = Monodoc.Lucene.Net.Index.Term;
namespace Monodoc.Lucene.Net.Search
{
	
    /// <summary>Implements the fuzzy search query. The similiarity measurement
    /// is based on the Levenshtein (edit distance) algorithm.
    /// </summary>
    [Serializable]
    public sealed class FuzzyQuery:MultiTermQuery
    {
		
        public const float defaultMinSimilarity = 0.5f;
        private float minimumSimilarity;
        private int prefixLength;
		
        /// <summary> Create a new FuzzyQuery that will match terms with a similarity 
        /// of at least <code>minimumSimilarity</code> to <code>term</code>.
        /// If a <code>prefixLength</code> &gt; 0 is specified, a common prefix
        /// of that length is also required.
        /// 
        /// </summary>
        /// <param name="term">the term to search for
        /// </param>
        /// <param name="minimumSimilarity">a value between 0 and 1 to set the required similarity
        /// between the query term and the matching terms. For example, for a
        /// <code>minimumSimilarity</code> of <code>0.5</code> a term of the same length
        /// as the query term is considered similar to the query term if the edit distance
        /// between both terms is less than <code>length(term)*0.5</code>
        /// </param>
        /// <param name="prefixLength">length of common (non-fuzzy) prefix
        /// </param>
        /// <throws>  IllegalArgumentException if minimumSimilarity is &gt; 1 or &lt; 0 </throws>
        /// <summary> or if prefixLength &lt; 0 or &gt; <code>term.text().length()</code>.
        /// </summary>
        public FuzzyQuery(Term term, float minimumSimilarity, int prefixLength):base(term)
        {
			
            if (minimumSimilarity > 1.0f)
                throw new System.ArgumentException("minimumSimilarity > 1");
            else if (minimumSimilarity < 0.0f)
                throw new System.ArgumentException("minimumSimilarity < 0");
            this.minimumSimilarity = minimumSimilarity;
			
            if (prefixLength < 0)
                throw new System.ArgumentException("prefixLength < 0");
            else if (prefixLength >= term.Text().Length)
                throw new System.ArgumentException("prefixLength >= term.text().length()");
            this.prefixLength = prefixLength;
        }
		
        /// <summary> Calls {@link #FuzzyQuery(Term, float) FuzzyQuery(term, minimumSimilarity, 0)}.</summary>
        public FuzzyQuery(Term term, float minimumSimilarity):this(term, minimumSimilarity, 0)
        {
        }
		
        /// <summary> Calls {@link #FuzzyQuery(Term, float) FuzzyQuery(term, 0.5f, 0)}.</summary>
        public FuzzyQuery(Term term):this(term, defaultMinSimilarity, 0)
        {
        }
		
        /// <summary> Returns the minimum similarity that is required for this query to match.</summary>
        /// <returns> float value between 0.0 and 1.0
        /// </returns>
        public float GetMinSimilarity()
        {
            return minimumSimilarity;
        }
		
        /// <summary> Returns the prefix length, i.e. the number of characters at the start
        /// of a term that must be identical (not fuzzy) to the query term if the query
        /// is to match that term. 
        /// </summary>
        public int GetPrefixLength()
        {
            return prefixLength;
        }
		
        protected internal override FilteredTermEnum GetEnum(Monodoc.Lucene.Net.Index.IndexReader reader)
        {
            return new FuzzyTermEnum(reader, GetTerm(), minimumSimilarity, prefixLength);
        }
		
        public override System.String ToString(System.String field)
        {
            return base.ToString(field) + '~' + minimumSimilarity.ToString();
        }
    }
}