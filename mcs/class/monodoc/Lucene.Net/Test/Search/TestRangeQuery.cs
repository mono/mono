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
using WhitespaceAnalyzer = Lucene.Net.Analysis.WhitespaceAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using NUnit.Framework;
namespace Lucene.Net.Search
{
	
	/// <author>  goller
	/// </author>
	[TestFixture]
    public class TestRangeQuery
	{
		
		private int docCount = 0;
		private RAMDirectory dir;
		
        [TestFixtureSetUp]
		public virtual void  SetUp()
		{
			dir = new RAMDirectory();
		}
		
        [Test]
		public virtual void  TestExclusive()
		{
			Query query = new RangeQuery(new Term("content", "A"), new Term("content", "C"), false);
			InitializeIndex(new System.String[]{"A", "B", "C", "D"});
			IndexSearcher searcher = new IndexSearcher(dir);
			Hits hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length(), "A,B,C,D, only B in range");
			searcher.Close();
			
			InitializeIndex(new System.String[]{"A", "B", "D"});
			searcher = new IndexSearcher(dir);
			hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length(), "A,B,D, only B in range");
			searcher.Close();
			
			AddDoc("C");
			searcher = new IndexSearcher(dir);
			hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length(), "C added, still only B in range");
			searcher.Close();
		}
		
        [Test]
		public virtual void  TestInclusive()
		{
			Query query = new RangeQuery(new Term("content", "A"), new Term("content", "C"), true);
			
			InitializeIndex(new System.String[]{"A", "B", "C", "D"});
			IndexSearcher searcher = new IndexSearcher(dir);
			Hits hits = searcher.Search(query);
			Assert.AreEqual(3, hits.Length(), "A,B,C,D - A,B,C in range");
			searcher.Close();
			
			InitializeIndex(new System.String[]{"A", "B", "D"});
			searcher = new IndexSearcher(dir);
			hits = searcher.Search(query);
			Assert.AreEqual(2, hits.Length(), "A,B,D - A and B in range");
			searcher.Close();
			
			AddDoc("C");
			searcher = new IndexSearcher(dir);
			hits = searcher.Search(query);
			Assert.AreEqual(3, hits.Length(), "C added - A, B, C in range");
			searcher.Close();
		}
		
		private void  InitializeIndex(System.String[] values)
		{
			IndexWriter writer = new IndexWriter(dir, new WhitespaceAnalyzer(), true);
			for (int i = 0; i < values.Length; i++)
			{
				InsertDoc(writer, values[i]);
			}
			writer.Close();
		}
		
		private void  AddDoc(System.String content)
		{
			IndexWriter writer = new IndexWriter(dir, new WhitespaceAnalyzer(), false);
			InsertDoc(writer, content);
			writer.Close();
		}
		
		private void  InsertDoc(IndexWriter writer, System.String content)
		{
			Document doc = new Document();
			
			doc.Add(Field.Keyword("id", "id" + docCount));
			doc.Add(Field.UnStored("content", content));
			
			writer.AddDocument(doc);
			docCount++;
		}
	}
}