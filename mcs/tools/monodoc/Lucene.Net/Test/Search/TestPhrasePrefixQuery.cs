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
using IndexReader = Lucene.Net.Index.IndexReader;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using TermEnum = Lucene.Net.Index.TermEnum;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using NUnit.Framework;
namespace Lucene.Net.Search
{
	
	/// <summary> This class tests PhrasePrefixQuery class.
	/// 
	/// </summary>
	/// <author>  Otis Gospodnetic
	/// </author>
	/// <version>  $Id: TestPhrasePrefixQuery.java,v 1.3 2004/03/29 22:48:06 cutting Exp $
	/// </version>
	[TestFixture]
    public class TestPhrasePrefixQuery
	{
		/// <summary> </summary>
		[Test]
		public virtual void  TestPhrasePrefix()
		{
			RAMDirectory indexStore = new RAMDirectory();
			IndexWriter writer = new IndexWriter(indexStore, new SimpleAnalyzer(), true);
			Document doc1 = new Document();
			Document doc2 = new Document();
			Document doc3 = new Document();
			Document doc4 = new Document();
			Document doc5 = new Document();
			doc1.Add(Field.Text("body", "blueberry pie"));
			doc2.Add(Field.Text("body", "blueberry strudel"));
			doc3.Add(Field.Text("body", "blueberry pizza"));
			doc4.Add(Field.Text("body", "blueberry chewing gum"));
			doc5.Add(Field.Text("body", "piccadilly circus"));
			writer.AddDocument(doc1);
			writer.AddDocument(doc2);
			writer.AddDocument(doc3);
			writer.AddDocument(doc4);
			writer.AddDocument(doc5);
			writer.Optimize();
			writer.Close();
			
			IndexSearcher searcher = new IndexSearcher(indexStore);
			
			PhrasePrefixQuery query1 = new PhrasePrefixQuery();
			PhrasePrefixQuery query2 = new PhrasePrefixQuery();
			query1.Add(new Term("body", "blueberry"));
			query2.Add(new Term("body", "strawberry"));
			
			System.Collections.ArrayList termsWithPrefix = new System.Collections.ArrayList();
			IndexReader ir = IndexReader.Open(indexStore);
			
			// this TermEnum gives "piccadilly", "pie" and "pizza".
			System.String prefix = "pi";
			TermEnum te = ir.Terms(new Term("body", prefix + "*"));
			do 
			{
				if (te.Term().Text().StartsWith(prefix))
				{
					termsWithPrefix.Add(te.Term());
				}
			}
			while (te.Next());
			
			query1.Add((Term[]) termsWithPrefix.ToArray(typeof(Term)));
			query2.Add((Term[]) termsWithPrefix.ToArray(typeof(Term)));
			
			Hits result;
			result = searcher.Search(query1);
			Assert.AreEqual(2, result.Length());
			
			result = searcher.Search(query2);
			Assert.AreEqual(0, result.Length());
		}
	}
}