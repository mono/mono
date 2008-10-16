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
using SimpleAnalyzer = Lucene.Net.Analysis.SimpleAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using QueryParser = Lucene.Net.QueryParsers.QueryParser;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Search
{
	
	/// <summary>Similarity unit test.
	/// 
	/// </summary>
	/// <author>  Doug Cutting
	/// </author>
	/// <version>  $Revision: 1.3 $
	/// </version>
	[TestFixture]
    public class TestNot
	{
        [Test]
		public virtual void  TestNot_()
		{
			RAMDirectory store = new RAMDirectory();
			IndexWriter writer = new IndexWriter(store, new SimpleAnalyzer(), true);
			
			Document d1 = new Document();
			d1.Add(Field.Text("Field", "a b"));
			
			writer.AddDocument(d1);
			writer.Optimize();
			writer.Close();
			
			Searcher searcher = new IndexSearcher(store);
			Query query = Lucene.Net.QueryParsers.QueryParser.Parse("a NOT b", "Field", new SimpleAnalyzer());
			//System.out.println(query);
			Hits hits = searcher.Search(query);
			Assert.AreEqual(0, hits.Length());
		}
	}
}