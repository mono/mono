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
	
	/// <author>  schnee
	/// </author>
	/// <version>  $Id: TestBooleanPrefixQuery.java,v 1.2 2004/03/29 22:48:06 cutting Exp $
	/// 
	/// </version>
	[TestFixture]
	public class TestBooleanPrefixQuery
	{
		
		[STAThread]
		public static void  Main(System.String[] args)
		{
			//TestRunner.run(Suite());
		}
		
        [Test]
		public virtual void  TestMethod()
		{
			RAMDirectory directory = new RAMDirectory();
			
			System.String[] categories = new System.String[]{"food", "foodanddrink", "foodanddrinkandgoodtimes", "food and drink"};
			
			Query rw1 = null;
			Query rw2 = null;
			try
			{
				IndexWriter writer = new IndexWriter(directory, new WhitespaceAnalyzer(), true);
				for (int i = 0; i < categories.Length; i++)
				{
					Document doc = new Document();
					doc.Add(Field.Keyword("category", categories[i]));
					writer.AddDocument(doc);
				}
				writer.Close();
				
				IndexReader reader = IndexReader.Open(directory);
				PrefixQuery query = new PrefixQuery(new Term("category", "foo"));
				
				rw1 = query.Rewrite(reader);
				
				BooleanQuery bq = new BooleanQuery();
				bq.Add(query, true, false);
				
				rw2 = bq.Rewrite(reader);
			}
			catch (System.IO.IOException e)
			{
				Assert.Fail(e.Message);
			}
			
			BooleanQuery bq1 = null;
			if (rw1 is BooleanQuery)
			{
				bq1 = (BooleanQuery) rw1;
			}
			
			BooleanQuery bq2 = null;
			if (rw2 is BooleanQuery)
			{
				bq2 = (BooleanQuery) rw2;
			}
			else
			{
				Assert.Fail("Rewrite");
			}
			
			Assert.AreEqual(bq1.GetClauses().Length, bq2.GetClauses().Length, "Number of Clauses Mismatch");
		}
	}
}