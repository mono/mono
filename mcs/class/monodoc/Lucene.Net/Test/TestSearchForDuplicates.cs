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
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParser;
using Lucene.Net.Search;
using Searchable = Lucene.Net.Search.Searchable;
using Lucene.Net.Store;
using NUnit.Framework;
namespace Lucene.Net
{
	
	
	/// <summary>JUnit adaptation of an older test case DocTest.</summary>
	/// <author>  dmitrys@earthlink.net
	/// </author>
	/// <version>  $Id: TestSearchForDuplicates.java,v 1.3 2004/03/29 22:48:05 cutting Exp $
	/// </version>
	[TestFixture]
    public class TestSearchForDuplicates
	{
		
		/// <summary>Main for running test case by itself. </summary>
		[STAThread]
		public static void  Main(System.String[] args)
		{
		}
		
		
		internal const System.String PRIORITY_FIELD = "priority";
		internal const System.String ID_FIELD = "id";
		internal const System.String HIGH_PRIORITY = "high";
		internal const System.String MED_PRIORITY = "medium";
		internal const System.String LOW_PRIORITY = "low";
		
		
		/// <summary>This test compares search results when using and not using compound
		/// files.
		/// 
		/// TODO: There is rudimentary search result validation as well, but it is
		/// simply based on asserting the output observed in the old test case,
		/// without really knowing if the output is correct. Someone needs to
		/// validate this output and make any changes to the checkHits method.
		/// </summary>
		[Test]
		public virtual void  TestRun()
		{
			System.IO.StringWriter sw = new System.IO.StringWriter();
			DoTest(sw, false);
			sw.Close();
			System.String multiFileOutput = sw.GetStringBuilder().ToString();
			//System.out.println(multiFileOutput);
			
			sw = new System.IO.StringWriter();
			DoTest(sw, true);
			sw.Close();
			System.String singleFileOutput = sw.GetStringBuilder().ToString();
			
			Assert.AreEqual(multiFileOutput, singleFileOutput);
		}
		
		
		private void  DoTest(System.IO.StringWriter out_Renamed, bool useCompoundFiles)
		{
			Directory directory = new RAMDirectory();
			Analyzer analyzer = new SimpleAnalyzer();
			IndexWriter writer = new IndexWriter(directory, analyzer, true);
			
			writer.SetUseCompoundFile(useCompoundFiles);
			
			int MAX_DOCS = 225;
			
			for (int j = 0; j < MAX_DOCS; j++)
			{
				Document d = new Document();
				d.Add(Field.Text(PRIORITY_FIELD, HIGH_PRIORITY));
				d.Add(Field.Text(ID_FIELD, System.Convert.ToString(j)));
				writer.AddDocument(d);
			}
			writer.Close();
			
			// try a search without OR
			Searcher searcher = new IndexSearcher(directory);
			Hits hits = null;
			
			QueryParsers.QueryParser parser = new QueryParsers.QueryParser(PRIORITY_FIELD, analyzer);
			
			Query query = parser.Parse(HIGH_PRIORITY);
			out_Renamed.WriteLine("Query: " + query.ToString(PRIORITY_FIELD));
			
			hits = searcher.Search(query);
			PrintHits(out_Renamed, hits);
			CheckHits(hits, MAX_DOCS);
			
			searcher.Close();
			
			// try a new search with OR
			searcher = new IndexSearcher(directory);
			hits = null;
			
			parser = new QueryParsers.QueryParser(PRIORITY_FIELD, analyzer);
			
			query = parser.Parse(HIGH_PRIORITY + " OR " + MED_PRIORITY);
			out_Renamed.WriteLine("Query: " + query.ToString(PRIORITY_FIELD));
			
			hits = searcher.Search(query);
			PrintHits(out_Renamed, hits);
			CheckHits(hits, MAX_DOCS);
			
			searcher.Close();
		}
		
		
		private void  PrintHits(System.IO.StringWriter out_Renamed, Hits hits)
		{
			out_Renamed.WriteLine(hits.Length() + " total results\n");
			for (int i = 0; i < hits.Length(); i++)
			{
				if (i < 10 || (i > 94 && i < 105))
				{
					Document d = hits.Doc(i);
					out_Renamed.WriteLine(i + " " + d.Get(ID_FIELD));
				}
			}
		}
		
		private void  CheckHits(Hits hits, int expectedCount)
		{
			Assert.AreEqual(expectedCount, hits.Length(), "total results");
			for (int i = 0; i < hits.Length(); i++)
			{
				if (i < 10 || (i > 94 && i < 105))
				{
					Document d = hits.Doc(i);
					Assert.AreEqual(System.Convert.ToString(i), d.Get(ID_FIELD), "check " + i);
				}
			}
		}
	}
}