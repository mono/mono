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
namespace Lucene.Net
{
	
	class SearchTestForDuplicates
	{
		
		internal const System.String PRIORITY_FIELD = "priority";
		internal const System.String ID_FIELD = "id";
		internal const System.String HIGH_PRIORITY = "high";
		internal const System.String MED_PRIORITY = "medium";
		internal const System.String LOW_PRIORITY = "low";
		
		[STAThread]
		public static void  Main(System.String[] args)
		{
			try
			{
				Directory directory = new RAMDirectory();
				Analyzer analyzer = new SimpleAnalyzer();
				IndexWriter writer = new IndexWriter(directory, analyzer, true);
				
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
				System.Console.Out.WriteLine("Query: " + query.ToString(PRIORITY_FIELD));
				
				hits = searcher.Search(query);
				PrintHits(hits);
				
				searcher.Close();
				
				// try a new search with OR
				searcher = new IndexSearcher(directory);
				hits = null;
				
				parser = new QueryParsers.QueryParser(PRIORITY_FIELD, analyzer);
				
				query = parser.Parse(HIGH_PRIORITY + " OR " + MED_PRIORITY);
				System.Console.Out.WriteLine("Query: " + query.ToString(PRIORITY_FIELD));
				
				hits = searcher.Search(query);
				PrintHits(hits);
				
				searcher.Close();
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
			}
		}
		
		private static void  PrintHits(Hits hits)
		{
			System.Console.Out.WriteLine(hits.Length() + " total results\n");
			for (int i = 0; i < hits.Length(); i++)
			{
				if (i < 10 || (i > 94 && i < 105))
				{
					Document d = hits.Doc(i);
					System.Console.Out.WriteLine(i + " " + d.Get(ID_FIELD));
				}
			}
		}
	}
}