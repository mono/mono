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
using CheckHits = Lucene.Net.Search.CheckHits;
using Hits = Lucene.Net.Search.Hits;
using IndexSearcher = Lucene.Net.Search.IndexSearcher;
using Query = Lucene.Net.Search.Query;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using NUnit.Framework;
namespace Lucene.Net.Search.Spans
{
	[TestFixture]
	public class TestSpans
	{
		private IndexSearcher searcher;
		
		public const System.String field = "Field";
		
        [TestFixtureSetUp]
		public virtual void  SetUp()
		{
			RAMDirectory directory = new RAMDirectory();
			IndexWriter writer = new IndexWriter(directory, new WhitespaceAnalyzer(), true);
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			for (int i = 0; i < docFields.Length; i++)
			{
				Document doc = new Document();
				doc.Add(Field.Text(field, docFields[i]));
				writer.AddDocument(doc);
			}
			writer.Close();
			searcher = new IndexSearcher(directory);
		}
		
		private System.String[] docFields = new System.String[]{"w1 w2 w3 w4 w5", "w1 w3 w2 w3", "w1 xx w2 yy w3", "w1 w3 xx w2 yy w3", ""};
		
		public virtual SpanTermQuery makeSpanTermQuery(System.String text)
		{
			return new SpanTermQuery(new Term(field, text));
		}
		
		private void  CheckHits(Query query, int[] results)
		{
			Lucene.Net.Search.CheckHits.CheckHits_(query, field, searcher, results, null);
		}
		
		public virtual void  OrderedSlopTest3(int slop, int[] expectedDocs)
		{
			SpanTermQuery w1 = makeSpanTermQuery("w1");
			SpanTermQuery w2 = makeSpanTermQuery("w2");
			SpanTermQuery w3 = makeSpanTermQuery("w3");
			bool ordered = true;
			SpanNearQuery snq = new SpanNearQuery(new SpanQuery[]{w1, w2, w3}, slop, ordered);
			CheckHits(snq, expectedDocs);
		}
		
        [Test]
		public virtual void  TestSpanNearOrdered01()
		{
			OrderedSlopTest3(0, new int[]{0});
		}
		
        [Test]
		public virtual void  TestSpanNearOrdered02()
		{
			OrderedSlopTest3(1, new int[]{0, 1});
		}
		
        [Test]
		public virtual void  TestSpanNearOrdered03()
		{
			OrderedSlopTest3(2, new int[]{0, 1, 2});
		}
		
        [Test]
		public virtual void  TestSpanNearOrdered04()
		{
			OrderedSlopTest3(3, new int[]{0, 1, 2, 3});
		}
		
        [Test]
		public virtual void  TestSpanNearOrdered05()
		{
			OrderedSlopTest3(4, new int[]{0, 1, 2, 3});
		}
	}
}