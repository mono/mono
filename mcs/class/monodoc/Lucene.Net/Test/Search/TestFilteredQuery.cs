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
using NUnit.Framework;
using WhitespaceAnalyzer = Lucene.Net.Analysis.WhitespaceAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexReader = Lucene.Net.Index.IndexReader;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Search
{
	/// <summary> FilteredQuery JUnit tests.
	/// 
	/// <p>Created: Apr 21, 2004 1:21:46 PM
	/// 
	/// </summary>
	/// <author>   Tim Jones
	/// </author>
	/// <version>  $Id: TestFilteredQuery.java,v 1.5 2004/07/10 06:19:01 otis Exp $
	/// </version>
	/// <since>   1.4
	/// </since>
	[TestFixture]
    public class TestFilteredQuery
	{
		[Serializable]
		private class AnonymousClassFilter : Filter
		{
			public AnonymousClassFilter(TestFilteredQuery enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(TestFilteredQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private TestFilteredQuery enclosingInstance;
			public TestFilteredQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override System.Collections.BitArray Bits(IndexReader reader)
			{
				System.Collections.BitArray bitset = new System.Collections.BitArray((5 % 64 == 0?5 / 64:5 / 64 + 1) * 64);
				bitset.Set(1, true);
				bitset.Set(3, true);
				return bitset;
			}
		}
		
		private IndexSearcher searcher;
		private RAMDirectory directory;
		private Query query;
		private Filter filter;
		
        [TestFixtureSetUp]
		public virtual void  SetUp()
		{
			directory = new RAMDirectory();
			IndexWriter writer = new IndexWriter(directory, new WhitespaceAnalyzer(), true);
			
			Document doc = new Document();
			doc.Add(Field.Text("Field", "one two three four five"));
			doc.Add(Field.Text("sorter", "b"));
			writer.AddDocument(doc);
			
			doc = new Document();
			doc.Add(Field.Text("Field", "one two three four"));
			doc.Add(Field.Text("sorter", "d"));
			writer.AddDocument(doc);
			
			doc = new Document();
			doc.Add(Field.Text("Field", "one two three y"));
			doc.Add(Field.Text("sorter", "a"));
			writer.AddDocument(doc);
			
			doc = new Document();
			doc.Add(Field.Text("Field", "one two x"));
			doc.Add(Field.Text("sorter", "c"));
			writer.AddDocument(doc);
			
			writer.Optimize();
			writer.Close();
			
			searcher = new IndexSearcher(directory);
			query = new TermQuery(new Term("Field", "three"));
			filter = new AnonymousClassFilter(this);
		}
		
        [TestFixtureTearDown]
		public virtual void  TearDown()
		{
			searcher.Close();
			directory.Close();
		}
		
        [Test]
		public virtual void  TestFilteredQuery_()
		{
			Query filteredquery = new FilteredQuery(query, filter);
			Hits hits = searcher.Search(filteredquery);
			Assert.AreEqual(1, hits.Length());
			Assert.AreEqual(1, hits.Id(0));
			
			hits = searcher.Search(filteredquery, new Sort("sorter"));
			Assert.AreEqual(1, hits.Length());
			Assert.AreEqual(1, hits.Id(0));
			
			filteredquery = new FilteredQuery(new TermQuery(new Term("Field", "one")), filter);
			hits = searcher.Search(filteredquery);
			Assert.AreEqual(2, hits.Length());
			
			filteredquery = new FilteredQuery(new TermQuery(new Term("Field", "x")), filter);
			hits = searcher.Search(filteredquery);
			Assert.AreEqual(1, hits.Length());
			Assert.AreEqual(3, hits.Id(0));
			
			filteredquery = new FilteredQuery(new TermQuery(new Term("Field", "y")), filter);
			hits = searcher.Search(filteredquery);
			Assert.AreEqual(0, hits.Length());
		}
		
		/// <summary> This tests FilteredQuery's rewrite correctness</summary>
		[Test]
		public virtual void  TestRangeQuery()
		{
			RangeQuery rq = new RangeQuery(new Term("sorter", "b"), new Term("sorter", "d"), true);
			
			Query filteredquery = new FilteredQuery(rq, filter);
			Hits hits = searcher.Search(filteredquery);
			Assert.AreEqual(2, hits.Length());
		}
	}
}