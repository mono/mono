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
using StandardAnalyzer = Lucene.Net.Analysis.Standard.StandardAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexReader = Lucene.Net.Index.IndexReader;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using QueryParser = Lucene.Net.QueryParsers.QueryParser;
using Directory = Lucene.Net.Store.Directory;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using NUnit.Framework;
namespace Lucene.Net.Search
{
	
	/// <summary> Tests {@link MultiSearcher} class.
	/// 
	/// </summary>
	/// <version>  $Id: TestMultiSearcher.java,v 1.6 2004/03/02 13:09:57 otis Exp $
	/// </version>
	[TestFixture]
    public class TestMultiSearcher
	{
		/// <summary> ReturnS a new instance of the concrete MultiSearcher class
		/// used in this test.
		/// </summary>
		protected internal virtual MultiSearcher GetMultiSearcherInstance(Searcher[] searchers)
		{
			return new MultiSearcher(searchers);
		}
		
        [Test]
		public virtual void  TestEmptyIndex()
		{
			// creating two directories for indices
			Directory indexStoreA = new RAMDirectory();
			Directory indexStoreB = new RAMDirectory();
			
			// creating a document to store
			Document lDoc = new Document();
			lDoc.Add(Field.Text("fulltext", "Once upon a time....."));
			lDoc.Add(Field.Keyword("id", "doc1"));
			lDoc.Add(Field.Keyword("handle", "1"));
			
			// creating a document to store
			Document lDoc2 = new Document();
			lDoc2.Add(Field.Text("fulltext", "in a galaxy far far away....."));
			lDoc2.Add(Field.Keyword("id", "doc2"));
			lDoc2.Add(Field.Keyword("handle", "1"));
			
			// creating a document to store
			Document lDoc3 = new Document();
			lDoc3.Add(Field.Text("fulltext", "a bizarre bug manifested itself...."));
			lDoc3.Add(Field.Keyword("id", "doc3"));
			lDoc3.Add(Field.Keyword("handle", "1"));
			
			// creating an index writer for the first index
			IndexWriter writerA = new IndexWriter(indexStoreA, new StandardAnalyzer(), true);
			// creating an index writer for the second index, but writing nothing
			IndexWriter writerB = new IndexWriter(indexStoreB, new StandardAnalyzer(), true);
			
			//--------------------------------------------------------------------
			// scenario 1
			//--------------------------------------------------------------------
			
			// writing the documents to the first index
			writerA.AddDocument(lDoc);
			writerA.AddDocument(lDoc2);
			writerA.AddDocument(lDoc3);
			writerA.Optimize();
			writerA.Close();
			
			// closing the second index
			writerB.Close();
			
			// creating the query
			Query query = Lucene.Net.QueryParsers.QueryParser.Parse("handle:1", "fulltext", new StandardAnalyzer());
			
			// building the searchables
			Searcher[] searchers = new Searcher[2];
			// VITAL STEP:adding the searcher for the empty index first, before the searcher for the populated index
			searchers[0] = new IndexSearcher(indexStoreB);
			searchers[1] = new IndexSearcher(indexStoreA);
			// creating the multiSearcher
			Searcher mSearcher = GetMultiSearcherInstance(searchers);
			// performing the search
			Hits hits = mSearcher.Search(query);
			
			Assert.AreEqual(3, hits.Length());
			
			try
			{
				// iterating over the hit documents
				for (int i = 0; i < hits.Length(); i++)
				{
					Document d = hits.Doc(i);
				}
			}
			catch (System.IndexOutOfRangeException e)
			{
				Assert.Fail("ArrayIndexOutOfBoundsException thrown: " + e.Message);
				System.Console.Error.WriteLine(e.Source);
			}
			finally
			{
				mSearcher.Close();
			}
			
			
			//--------------------------------------------------------------------
			// scenario 2
			//--------------------------------------------------------------------
			
			// adding one document to the empty index
			writerB = new IndexWriter(indexStoreB, new StandardAnalyzer(), false);
			writerB.AddDocument(lDoc);
			writerB.Optimize();
			writerB.Close();
			
			// building the searchables
			Searcher[] searchers2 = new Searcher[2];
			// VITAL STEP:adding the searcher for the empty index first, before the searcher for the populated index
			searchers2[0] = new IndexSearcher(indexStoreB);
			searchers2[1] = new IndexSearcher(indexStoreA);
			// creating the mulitSearcher
			Searcher mSearcher2 = GetMultiSearcherInstance(searchers2);
			// performing the same search
			Hits hits2 = mSearcher2.Search(query);
			
			Assert.AreEqual(4, hits2.Length());
			
			try
			{
				// iterating over the hit documents
				for (int i = 0; i < hits2.Length(); i++)
				{
					// no exception should happen at this point
					Document d = hits2.Doc(i);
				}
			}
			catch (System.Exception e)
			{
				Assert.Fail("Exception thrown: " + e.Message);
                System.Console.Error.WriteLine(e.Source);
            }
			finally
			{
				mSearcher2.Close();
			}
			
			//--------------------------------------------------------------------
			// scenario 3
			//--------------------------------------------------------------------
			
			// deleting the document just added, this will cause a different exception to take place
			Term term = new Term("id", "doc1");
			IndexReader readerB = IndexReader.Open(indexStoreB);
			readerB.Delete(term);
			readerB.Close();
			
			// optimizing the index with the writer
			writerB = new IndexWriter(indexStoreB, new StandardAnalyzer(), false);
			writerB.Optimize();
			writerB.Close();
			
			// building the searchables
			Searcher[] searchers3 = new Searcher[2];
			
			searchers3[0] = new IndexSearcher(indexStoreB);
			searchers3[1] = new IndexSearcher(indexStoreA);
			// creating the mulitSearcher
			Searcher mSearcher3 = GetMultiSearcherInstance(searchers3);
			// performing the same search
			Hits hits3 = mSearcher3.Search(query);
			
			Assert.AreEqual(3, hits3.Length());
			
			try
			{
				// iterating over the hit documents
				for (int i = 0; i < hits3.Length(); i++)
				{
					Document d = hits3.Doc(i);
				}
			}
			catch (System.IO.IOException e)
			{
				Assert.Fail("IOException thrown: " + e.Message);
                System.Console.Error.WriteLine(e.Source);
            }
			finally
			{
				mSearcher3.Close();
			}
		}
	}
}