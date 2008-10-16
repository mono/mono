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
using SimpleAnalyzer = Lucene.Net.Analysis.SimpleAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using NUnit.Framework;
namespace Lucene.Net.Search
{
	
	/// <summary> TestWildcard tests the '*' and '?' wildard characters.
	/// 
	/// </summary>
	/// <author>  Otis Gospodnetic
	/// </author>
	[TestFixture]
    public class TestWildcard
	{
		/// <summary> Creates a new <code>TestWildcard</code> instance.
		/// 
		/// </summary>
		/// <param name="name">the name of the test
		/// </param>
		
		/// <summary> Tests Wildcard queries with an asterisk.
		/// 
		/// </summary>
		[Test]
		public virtual void  TestAsterisk()
		{
			RAMDirectory indexStore = GetIndexStore("body", new System.String[]{"metal", "metals"});
			IndexSearcher searcher = new IndexSearcher(indexStore);
			Query query1 = new TermQuery(new Term("body", "metal"));
			Query query2 = new WildcardQuery(new Term("body", "metal*"));
			Query query3 = new WildcardQuery(new Term("body", "m*tal"));
			Query query4 = new WildcardQuery(new Term("body", "m*tal*"));
			Query query5 = new WildcardQuery(new Term("body", "m*tals"));
			
			BooleanQuery query6 = new BooleanQuery();
			query6.Add(query5, false, false);
			
			BooleanQuery query7 = new BooleanQuery();
			query7.Add(query3, false, false);
			query7.Add(query5, false, false);
			
			// Queries do not automatically lower-case search terms:
			Query query8 = new WildcardQuery(new Term("body", "M*tal*"));
			
			AssertMatches(searcher, query1, 1);
			AssertMatches(searcher, query2, 2);
			AssertMatches(searcher, query3, 1);
			AssertMatches(searcher, query4, 2);
			AssertMatches(searcher, query5, 1);
			AssertMatches(searcher, query6, 1);
			AssertMatches(searcher, query7, 2);
			AssertMatches(searcher, query8, 0);
		}
		
		/// <summary> Tests Wildcard queries with a question mark.
		/// 
		/// </summary>
		/// <exception cref=""> IOException if an error occurs
		/// </exception>
		[Test]
        public virtual void  TestQuestionmark()
		{
			RAMDirectory indexStore = GetIndexStore("body", new System.String[]{"metal", "metals", "mXtals", "mXtXls"});
			IndexSearcher searcher = new IndexSearcher(indexStore);
			Query query1 = new WildcardQuery(new Term("body", "m?tal"));
			Query query2 = new WildcardQuery(new Term("body", "metal?"));
			Query query3 = new WildcardQuery(new Term("body", "metals?"));
			Query query4 = new WildcardQuery(new Term("body", "m?t?ls"));
			Query query5 = new WildcardQuery(new Term("body", "M?t?ls"));
			
			AssertMatches(searcher, query1, 1);
			AssertMatches(searcher, query2, 2);
			AssertMatches(searcher, query3, 1);
			AssertMatches(searcher, query4, 3);
			AssertMatches(searcher, query5, 0);
		}
		
		private RAMDirectory GetIndexStore(System.String field, System.String[] contents)
		{
			RAMDirectory indexStore = new RAMDirectory();
			IndexWriter writer = new IndexWriter(indexStore, new SimpleAnalyzer(), true);
			for (int i = 0; i < contents.Length; ++i)
			{
				Document doc = new Document();
				doc.Add(Field.Text(field, contents[i]));
				writer.AddDocument(doc);
			}
			writer.Optimize();
			writer.Close();
			
			return indexStore;
		}
		
		private void  AssertMatches(IndexSearcher searcher, Query q, int expectedMatches)
		{
			Hits result = searcher.Search(q);
			Assert.AreEqual(expectedMatches, result.Length());
		}
	}
}