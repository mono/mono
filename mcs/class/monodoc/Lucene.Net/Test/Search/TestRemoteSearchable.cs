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
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Search
{
	
	/// <version>  $Id: TestRemoteSearchable.java,v 1.7 2004/03/29 22:48:06 cutting Exp $
	/// </version>
	[TestFixture]
    public class TestRemoteSearchable
	{
		private static Lucene.Net.Search.Searchable Remote
		{
			get
			{
				try
				{
					return LookupRemote();
				}
				catch (System.Exception e)
				{
					StartServer();
					return LookupRemote();
				}
			}
			
		}
		
		private static Lucene.Net.Search.Searchable LookupRemote()
		{
			return (Lucene.Net.Search.Searchable) Activator.GetObject(typeof(Lucene.Net.Search.Searchable), "http://localhost/Searchable");
		}
		
		private static void  StartServer()
		{
			// construct an index
			RAMDirectory indexStore = new RAMDirectory();
			IndexWriter writer = new IndexWriter(indexStore, new SimpleAnalyzer(), true);
			Document doc = new Document();
			doc.Add(Field.Text("test", "test text"));
			writer.AddDocument(doc);
			writer.Optimize();
			writer.Close();
			
			// publish it
			//// LocateRegistry.CreateRegistry(1099); // {{Aroush}}
			Lucene.Net.Search.Searchable local = new IndexSearcher(indexStore);
			RemoteSearchable impl = new RemoteSearchable(local);
			System.Runtime.Remoting.RemotingServices.Marshal(impl, "http://localhost/Searchable");
		}
		
		private static void  Search(Query query)
		{
			// try to search the published index
			Lucene.Net.Search.Searchable[] searchables = new Lucene.Net.Search.Searchable[]{Remote};
			Searcher searcher = new MultiSearcher(searchables);
			Hits result = searcher.Search(query);
			
			Assert.AreEqual(1, result.Length());
			Assert.AreEqual("test text", result.Doc(0).Get("test"));
		}
		
        [Test]
		public virtual void  TestTermQuery()
		{
			Search(new TermQuery(new Term("test", "test")));
		}
		
        [Test]
		public virtual void  TestBooleanQuery()
		{
			BooleanQuery query = new BooleanQuery();
			query.Add(new TermQuery(new Term("test", "test")), true, false);
			Search(query);
		}
		
        [Test]
		public virtual void  TestPhraseQuery()
		{
			PhraseQuery query = new PhraseQuery();
			query.Add(new Term("test", "test"));
			query.Add(new Term("test", "text"));
			Search(query);
		}
		
		// Tests bug fix at http://nagoya.apache.org/bugzilla/show_bug.cgi?id=20290
        [Test]
		public virtual void  TestQueryFilter()
		{
			// try to search the published index
			Lucene.Net.Search.Searchable[] searchables = new Lucene.Net.Search.Searchable[]{Remote};
			Searcher searcher = new MultiSearcher(searchables);
			Hits hits = searcher.Search(new TermQuery(new Term("test", "text")), new QueryFilter(new TermQuery(new Term("test", "test"))));
			Hits nohits = searcher.Search(new TermQuery(new Term("test", "text")), new QueryFilter(new TermQuery(new Term("test", "non-existent-term"))));
			Assert.AreEqual(0, nohits.Length());
		}
	}
}