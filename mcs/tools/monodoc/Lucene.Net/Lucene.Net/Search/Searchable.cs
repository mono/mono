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
using Document = Monodoc.Lucene.Net.Documents.Document;
using Monodoc.Lucene.Net.Index;
using Term = Monodoc.Lucene.Net.Index.Term;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>The interface for search implementations.
	/// 
	/// <p>Implementations provide search over a single index, over multiple
	/// indices, and over indices on remote servers.
	/// </summary>
	
    public interface Searchable
    {
        /// <summary>Lower-level search API.
        /// 
        /// <p>{@link HitCollector#Collect(int,float)} is called for every non-zero
        /// scoring document.
        /// 
        /// <p>Applications should only use this if they need <i>all</i> of the
        /// matching documents.  The high-level search API ({@link
        /// Searcher#Search(Query)}) is usually more efficient, as it skips
        /// non-high-scoring hits.
        /// 
        /// </summary>
        /// <param name="query">to match documents
        /// </param>
        /// <param name="filter">if non-null, a bitset used to eliminate some documents
        /// </param>
        /// <param name="results">to receive hits
        /// </param>
        void  Search(Query query, Filter filter, HitCollector results);
		
        /// <summary>Frees resources associated with this Searcher.
        /// Be careful not to call this method while you are still using objects
        /// like {@link Hits}.
        /// </summary>
        void  Close();
		
        /// <summary>Expert: Returns the number of documents containing <code>term</code>.
        /// Called by search code to compute term weights.
        /// </summary>
        /// <seealso cref="Monodoc.Lucene.Net.Index.IndexReader#docFreq(Term).">
        /// </seealso>
        int DocFreq(Term term);
		
        /// <summary>Expert: Returns one greater than the largest possible document number.
        /// Called by search code to compute term weights.
        /// </summary>
        /// <seealso cref="Monodoc.Lucene.Net.Index.IndexReader#maxDoc().">
        /// </seealso>
        int MaxDoc();
		
        /// <summary>Expert: Low-level search implementation.  Finds the top <code>n</code>
        /// hits for <code>query</code>, applying <code>filter</code> if non-null.
        /// 
        /// <p>Called by {@link Hits}.
        /// 
        /// <p>Applications should usually call {@link Searcher#Search(Query)} or
        /// {@link Searcher#Search(Query,Filter)} instead.
        /// </summary>
        TopDocs Search(Query query, Filter filter, int n);
		
        /// <summary>Expert: Returns the stored fields of document <code>i</code>.
        /// Called by {@link HitCollector} implementations.
        /// </summary>
        /// <seealso cref="Monodoc.Lucene.Net.Index.IndexReader#document(int).">
        /// </seealso>
        Document Doc(int i);
		
        /// <summary>Expert: called to re-write queries into primitive queries. </summary>
        Query Rewrite(Query query);
		
        /// <summary>Returns an Explanation that describes how <code>doc</code> scored against
        /// <code>query</code>.
        /// 
        /// <p>This is intended to be used in developing Similarity implementations,
        /// and, for good performance, should not be displayed with every hit.
        /// Computing an explanation is as expensive as executing the query over the
        /// entire index.
        /// </summary>
        Explanation Explain(Query query, int doc);
		
        /// <summary>Expert: Low-level search implementation with arbitrary sorting.  Finds
        /// the top <code>n</code> hits for <code>query</code>, applying
        /// <code>filter</code> if non-null, and sorting the hits by the criteria in
        /// <code>sort</code>.
        /// 
        /// <p>Applications should usually call {@link
        /// Searcher#Search(Query,Filter,Sort)} instead.
        /// </summary>
        TopFieldDocs Search(Query query, Filter filter, int n, Sort sort);
    }
}