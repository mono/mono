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
using StandardAnalyzer = Lucene.Net.Analysis.Standard.StandardAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using Hits = Lucene.Net.Search.Hits;
using IndexSearcher = Lucene.Net.Search.IndexSearcher;
using Query = Lucene.Net.Search.Query;
using Searcher = Lucene.Net.Search.Searcher;
using TermQuery = Lucene.Net.Search.TermQuery;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Documents
{
	/// <summary> Tests {@link Document} class.
	/// 
	/// </summary>
	/// <author>  Otis Gospodnetic
	/// </author>
	/// <version>  $Id: TestDocument.java,v 1.4 2004/04/20 17:26:16 goller Exp $
	/// </version>
	[TestFixture]
	public class TestDocument
	{
		
		/// <summary> Tests {@link Document#remove()} method for a brand new Document
		/// that has not been indexed yet.
		/// 
		/// </summary>
		/// <throws>  Exception on error </throws>
		[Test]
		public virtual void  TestRemoveForNewDocument()
		{
			Document doc = MakeDocumentWithFields();
			Assert.AreEqual(8, doc.fields.Count);
			doc.RemoveFields("keyword");
			Assert.AreEqual(6, doc.fields.Count);
			doc.RemoveFields("doesnotexists"); // removing non-existing fields is siltenlty ignored
			doc.RemoveFields("keyword"); // removing a Field more than once
			Assert.AreEqual(6, doc.fields.Count);
			doc.RemoveField("text");
			Assert.AreEqual(5, doc.fields.Count);
			doc.RemoveField("text");
			Assert.AreEqual(4, doc.fields.Count);
			doc.RemoveField("text");
			Assert.AreEqual(4, doc.fields.Count);
			doc.RemoveField("doesnotexists"); // removing non-existing fields is siltenlty ignored
			Assert.AreEqual(4, doc.fields.Count);
			doc.RemoveFields("unindexed");
			Assert.AreEqual(2, doc.fields.Count);
			doc.RemoveFields("unstored");
			Assert.AreEqual(0, doc.fields.Count);
			doc.RemoveFields("doesnotexists"); // removing non-existing fields is siltenlty ignored
			Assert.AreEqual(0, doc.fields.Count);
		}
		
		/// <summary> Tests {@link Document#getValues()} method for a brand new Document
		/// that has not been indexed yet.
		/// 
		/// </summary>
		/// <throws>  Exception on error </throws>
		[Test]
		public virtual void  TestGetValuesForNewDocument()
		{
			DoAssert(MakeDocumentWithFields(), false);
		}
		
		/// <summary> Tests {@link Document#getValues()} method for a Document retrieved from
		/// an index.
		/// 
		/// </summary>
		/// <throws>  Exception on error </throws>
		[Test]
        public virtual void  TestGetValuesForIndexedDocument()
		{
			RAMDirectory dir = new RAMDirectory();
			IndexWriter writer = new IndexWriter(dir, new StandardAnalyzer(), true);
			writer.AddDocument(MakeDocumentWithFields());
			writer.Close();
			
			Searcher searcher = new IndexSearcher(dir);
			
			// search for something that does exists
			Query query = new TermQuery(new Term("keyword", "test1"));
			
			// ensure that queries return expected results without DateFilter first
			Hits hits = searcher.Search(query);
			Assert.AreEqual(1, hits.Length());
			
			try
			{
				DoAssert(hits.Doc(0), true);
			}
			catch (System.Exception e)
			{
                System.Console.Error.WriteLine(e.StackTrace);
				System.Console.Error.Write("\n");
			}
			finally
			{
				searcher.Close();
			}
		}
		
		private Document MakeDocumentWithFields()
		{
			Document doc = new Document();
			doc.Add(Field.Keyword("keyword", "test1"));
			doc.Add(Field.Keyword("keyword", "test2"));
			doc.Add(Field.Text("text", "test1"));
			doc.Add(Field.Text("text", "test2"));
			doc.Add(Field.UnIndexed("unindexed", "test1"));
			doc.Add(Field.UnIndexed("unindexed", "test2"));
			doc.Add(Field.UnStored("unstored", "test1"));
			doc.Add(Field.UnStored("unstored", "test2"));
			return doc;
		}
		
		private void  DoAssert(Document doc, bool fromIndex)
		{
			System.String[] keywordFieldValues = doc.GetValues("keyword");
			System.String[] textFieldValues = doc.GetValues("text");
			System.String[] unindexedFieldValues = doc.GetValues("unindexed");
			System.String[] unstoredFieldValues = doc.GetValues("unstored");
			
			Assert.IsTrue(keywordFieldValues.Length == 2);
			Assert.IsTrue(textFieldValues.Length == 2);
			Assert.IsTrue(unindexedFieldValues.Length == 2);
			// this test cannot work for documents retrieved from the index
			// since unstored fields will obviously not be returned
			if (!fromIndex)
			{
				Assert.IsTrue(unstoredFieldValues.Length == 2);
			}
			
			Assert.IsTrue(keywordFieldValues[0].Equals("test1"));
			Assert.IsTrue(keywordFieldValues[1].Equals("test2"));
			Assert.IsTrue(textFieldValues[0].Equals("test1"));
			Assert.IsTrue(textFieldValues[1].Equals("test2"));
			Assert.IsTrue(unindexedFieldValues[0].Equals("test1"));
			Assert.IsTrue(unindexedFieldValues[1].Equals("test2"));
			// this test cannot work for documents retrieved from the index
			// since unstored fields will obviously not be returned
			if (!fromIndex)
			{
				Assert.IsTrue(unstoredFieldValues[0].Equals("test1"));
				Assert.IsTrue(unstoredFieldValues[1].Equals("test2"));
			}
		}
	}
}