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
using DateField = Lucene.Net.Documents.DateField;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using NUnit.Framework;
namespace Lucene.Net.Search
{
	
	/// <summary> DateFilter JUnit tests.
	/// 
	/// </summary>
	/// <author>  Otis Gospodnetic
	/// </author>
	/// <version>  $Revision: 1.5 $
	/// </version>
	[TestFixture]
	public class TestDateFilter
	{
		/// <summary> </summary>
		[Test]
		public virtual void  TestBefore()
		{
			// create an index
			RAMDirectory indexStore = new RAMDirectory();
			IndexWriter writer = new IndexWriter(indexStore, new SimpleAnalyzer(), true);
			
			long now = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
			
			Document doc = new Document();
			// add time that is in the past
			doc.Add(Field.Keyword("datefield", DateField.TimeToString(now - 1000)));
			doc.Add(Field.Text("body", "Today is a very sunny day in New York City"));
			writer.AddDocument(doc);
			writer.Optimize();
			writer.Close();
			
			IndexSearcher searcher = new IndexSearcher(indexStore);
			
			// filter that should preserve matches
			DateFilter df1 = DateFilter.Before("datefield", now);
			
			// filter that should discard matches
			DateFilter df2 = DateFilter.Before("datefield", now - 999999);
			
			// search something that doesn't exist with DateFilter
			Query query1 = new TermQuery(new Term("body", "NoMatchForThis"));
			
			// search for something that does exists
			Query query2 = new TermQuery(new Term("body", "sunny"));
			
			Hits result;
			
			// ensure that queries return expected results without DateFilter first
			result = searcher.Search(query1);
			Assert.AreEqual(0, result.Length());
			
			result = searcher.Search(query2);
			Assert.AreEqual(1, result.Length());
			
			
			// run queries with DateFilter
			result = searcher.Search(query1, df1);
			Assert.AreEqual(0, result.Length());
			
			result = searcher.Search(query1, df2);
			Assert.AreEqual(0, result.Length());
			
			result = searcher.Search(query2, df1);
			Assert.AreEqual(1, result.Length());
			
			result = searcher.Search(query2, df2);
			Assert.AreEqual(0, result.Length());
		}
		
		/// <summary> </summary>
		[Test]
		public virtual void  TestAfter()
		{
			// create an index
			RAMDirectory indexStore = new RAMDirectory();
			IndexWriter writer = new IndexWriter(indexStore, new SimpleAnalyzer(), true);
			
			long now = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
			
			Document doc = new Document();
			// add time that is in the future
			doc.Add(Field.Keyword("datefield", DateField.TimeToString(now + 888888)));
			doc.Add(Field.Text("body", "Today is a very sunny day in New York City"));
			writer.AddDocument(doc);
			writer.Optimize();
			writer.Close();
			
			IndexSearcher searcher = new IndexSearcher(indexStore);
			
			// filter that should preserve matches
			DateFilter df1 = DateFilter.After("datefield", now);
			
			// filter that should discard matches
			DateFilter df2 = DateFilter.After("datefield", now + 999999);
			
			// search something that doesn't exist with DateFilter
			Query query1 = new TermQuery(new Term("body", "NoMatchForThis"));
			
			// search for something that does exists
			Query query2 = new TermQuery(new Term("body", "sunny"));
			
			Hits result;
			
			// ensure that queries return expected results without DateFilter first
			result = searcher.Search(query1);
			Assert.AreEqual(0, result.Length());
			
			result = searcher.Search(query2);
			Assert.AreEqual(1, result.Length());
			
			
			// run queries with DateFilter
			result = searcher.Search(query1, df1);
			Assert.AreEqual(0, result.Length());
			
			result = searcher.Search(query1, df2);
			Assert.AreEqual(0, result.Length());
			
			result = searcher.Search(query2, df1);
			Assert.AreEqual(1, result.Length());
			
			result = searcher.Search(query2, df2);
			Assert.AreEqual(0, result.Length());
		}
	}
}