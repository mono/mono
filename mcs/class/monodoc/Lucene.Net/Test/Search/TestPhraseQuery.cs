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
using StopAnalyzer = Lucene.Net.Analysis.StopAnalyzer;
using WhitespaceAnalyzer = Lucene.Net.Analysis.WhitespaceAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Search
{
	
	/// <summary> Tests {@link PhraseQuery}.
	/// 
	/// </summary>
	/// <seealso cref="TestPositionIncrement">
	/// </seealso>
	/// <author>  Erik Hatcher
	/// </author>
	[TestFixture]
    public class TestPhraseQuery
	{
		private IndexSearcher searcher;
		private PhraseQuery query;
		private RAMDirectory directory;
		
        [TestFixtureSetUp]
		public virtual void  SetUp()
		{
			directory = new RAMDirectory();
			IndexWriter writer = new IndexWriter(directory, new WhitespaceAnalyzer(), true);
			
			Document doc = new Document();
			doc.Add(Field.Text("Field", "one two three four five"));
			writer.AddDocument(doc);
			
			writer.Optimize();
			writer.Close();
			
			searcher = new IndexSearcher(directory);
			query = new PhraseQuery();
		}
		
        [TestFixtureTearDown]
		public virtual void  TearDown()
		{
			searcher.Close();
			directory.Close();
		}
		
        [Test]
		public virtual void  TestNotCloseEnough()
		{
			query.SetSlop(2);
			query.Add(new Term("Field", "one"));
			query.Add(new Term("Field", "five"));
			Hits hits = searcher.Search(query);
			Assert.AreEqual(0, hits.Length());
		}
		
        [Test]
		public virtual void  TestBarelyCloseEnough()
		{
			query.SetSlop(3);
			query.Add(new Term("Field", "one"));
			query.Add(new Term("Field", "five"));
			Hits hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length());
		}
		
		/// <summary> Ensures slop of 0 works for exact matches, but not reversed</summary>
		[Test]
		public virtual void  TestExact()
		{
			// slop is zero by default
			query.Add(new Term("Field", "four"));
			query.Add(new Term("Field", "five"));
			Hits hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length(), "exact match");
			
			query = new PhraseQuery();
			query.Add(new Term("Field", "two"));
			query.Add(new Term("Field", "one"));
			hits = searcher.Search(query);
			Assert.AreEqual(0, hits.Length(), "reverse not exact");
		}
		
        [Test]
		public virtual void  TestSlop1()
		{
            SetUp();

			// Ensures slop of 1 works with terms in order.
			query.SetSlop(1);
			query.Add(new Term("Field", "one"));
			query.Add(new Term("Field", "two"));
			Hits hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length(), "in order");
			
			// Ensures slop of 1 does not work for phrases out of order;
			// must be at least 2.
			query = new PhraseQuery();
			query.SetSlop(1);
			query.Add(new Term("Field", "two"));
			query.Add(new Term("Field", "one"));
			hits = searcher.Search(query);
			Assert.AreEqual(0, hits.Length(), "reversed, slop not 2 or more");
		}
		
		/// <summary> As long as slop is at least 2, terms can be reversed</summary>
		[Test]
        public virtual void  TestOrderDoesntMatter()
		{
            SetUp();

			query.SetSlop(2); // must be at least two for reverse order match
			query.Add(new Term("Field", "two"));
			query.Add(new Term("Field", "one"));
			Hits hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length(), "just sloppy enough");
			
			query = new PhraseQuery();
			query.SetSlop(2);
			query.Add(new Term("Field", "three"));
			query.Add(new Term("Field", "one"));
			hits = searcher.Search(query);
			Assert.AreEqual(0, hits.Length(), "not sloppy enough");
		}
		
		/// <summary> slop is the total number of positional moves allowed
		/// to line up a phrase
		/// </summary>
		[Test]
        public virtual void  TestMulipleTerms()
		{
            SetUp();

			query.SetSlop(2);
			query.Add(new Term("Field", "one"));
			query.Add(new Term("Field", "three"));
			query.Add(new Term("Field", "five"));
			Hits hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length(), "two total moves");
			
			query = new PhraseQuery();
			query.SetSlop(5); // it takes six moves to match this phrase
			query.Add(new Term("Field", "five"));
			query.Add(new Term("Field", "three"));
			query.Add(new Term("Field", "one"));
			hits = searcher.Search(query);
			Assert.AreEqual(0, hits.Length(), "slop of 5 not close enough");
			
			query.SetSlop(6);
			hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length(), "slop of 6 just right");
		}
		
        [Test]
		public virtual void  TestPhraseQueryWithStopAnalyzer()
		{
			RAMDirectory directory = new RAMDirectory();
			StopAnalyzer stopAnalyzer = new StopAnalyzer();
			IndexWriter writer = new IndexWriter(directory, stopAnalyzer, true);
			Document doc = new Document();
			doc.Add(Field.Text("Field", "the stop words are here"));
			writer.AddDocument(doc);
			writer.Close();
			
			IndexSearcher searcher = new IndexSearcher(directory);
			
			// valid exact phrase query
			PhraseQuery query = new PhraseQuery();
			query.Add(new Term("Field", "stop"));
			query.Add(new Term("Field", "words"));
			Hits hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length());
			
			// currently StopAnalyzer does not leave "holes", so this matches.
			query = new PhraseQuery();
			query.Add(new Term("Field", "words"));
			query.Add(new Term("Field", "here"));
			hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length());
			
			searcher.Close();
		}
		
        [Test]
		public virtual void  TestPhraseQueryInConjunctionScorer()
		{
			RAMDirectory directory = new RAMDirectory();
			IndexWriter writer = new IndexWriter(directory, new WhitespaceAnalyzer(), true);
			
			Document doc = new Document();
			doc.Add(new Field("source", "marketing info", true, true, true));
			writer.AddDocument(doc);
			
			doc = new Document();
			doc.Add(new Field("contents", "foobar", true, true, true));
			doc.Add(new Field("source", "marketing info", true, true, true));
			writer.AddDocument(doc);
			
			writer.Optimize();
			writer.Close();
			
			IndexSearcher searcher = new IndexSearcher(directory);
			
			PhraseQuery phraseQuery = new PhraseQuery();
			phraseQuery.Add(new Term("source", "marketing"));
			phraseQuery.Add(new Term("source", "info"));
			Hits hits = searcher.Search(phraseQuery);
			Assert.AreEqual(2, hits.Length());
			
			TermQuery termQuery = new TermQuery(new Term("contents", "foobar"));
			BooleanQuery booleanQuery = new BooleanQuery();
			booleanQuery.Add(termQuery, true, false);
			booleanQuery.Add(phraseQuery, true, false);
			hits = searcher.Search(booleanQuery);
			Assert.AreEqual(1, hits.Length());
			
			searcher.Close();
			
			writer = new IndexWriter(directory, new WhitespaceAnalyzer(), true);
			doc = new Document();
			doc.Add(new Field("contents", "map entry woo", true, true, true));
			writer.AddDocument(doc);
			
			doc = new Document();
			doc.Add(new Field("contents", "woo map entry", true, true, true));
			writer.AddDocument(doc);
			
			doc = new Document();
			doc.Add(new Field("contents", "map foobarword entry woo", true, true, true));
			writer.AddDocument(doc);
			
			writer.Optimize();
			writer.Close();
			
			searcher = new IndexSearcher(directory);
			
			termQuery = new TermQuery(new Term("contents", "woo"));
			phraseQuery = new PhraseQuery();
			phraseQuery.Add(new Term("contents", "map"));
			phraseQuery.Add(new Term("contents", "entry"));
			
			hits = searcher.Search(termQuery);
			Assert.AreEqual(3, hits.Length());
			hits = searcher.Search(phraseQuery);
			Assert.AreEqual(2, hits.Length());
			
			booleanQuery = new BooleanQuery();
			booleanQuery.Add(termQuery, true, false);
			booleanQuery.Add(phraseQuery, true, false);
			hits = searcher.Search(booleanQuery);
			Assert.AreEqual(2, hits.Length());
			
			booleanQuery = new BooleanQuery();
			booleanQuery.Add(phraseQuery, true, false);
			booleanQuery.Add(termQuery, true, false);
			hits = searcher.Search(booleanQuery);
			Assert.AreEqual(2, hits.Length());
			
			searcher.Close();
			directory.Close();
		}
	}
}