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
	
	class SearchTest
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			try
			{
				Directory directory = new RAMDirectory();
				Analyzer analyzer = new SimpleAnalyzer();
				IndexWriter writer = new IndexWriter(directory, analyzer, true);
				
				System.String[] docs = new System.String[]{"a b c d e", "a b c d e a b c d e", "a b c d e f g h i j", "a c e", "e c a", "a c e a c e", "a c e a b c"};
				for (int j = 0; j < docs.Length; j++)
				{
					Document d = new Document();
					d.Add(Field.Text("contents", docs[j]));
					writer.AddDocument(d);
				}
				writer.Close();
				
				Searcher searcher = new IndexSearcher(directory);
				
				System.String[] queries = new System.String[]{"\"a c e\""};
				Hits hits = null;
				
				QueryParsers.QueryParser parser = new QueryParsers.QueryParser("contents", analyzer);
				parser.SetPhraseSlop(4);
				for (int j = 0; j < queries.Length; j++)
				{
					Query query = parser.Parse(queries[j]);
					System.Console.Out.WriteLine("Query: " + query.ToString("contents"));
					
					//DateFilter filter =
					//  new DateFilter("modified", Time(1997,0,1), Time(1998,0,1));
					//DateFilter filter = DateFilter.Before("modified", Time(1997,00,01));
					//System.out.println(filter);
					
					hits = searcher.Search(query);
					
					System.Console.Out.WriteLine(hits.Length() + " total results");
					for (int i = 0; i < hits.Length() && i < 10; i++)
					{
						Document d = hits.Doc(i);
						System.Console.Out.WriteLine(i + " " + hits.Score(i) + " " + d.Get("contents"));
					}
				}
				searcher.Close();
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
			}
		}
		
		internal static long Time(int year, int month, int day)
		{
            // {{Aroush
            System.DateTime tempDate = System.DateTime.Now;
			System.Globalization.GregorianCalendar calendar = new System.Globalization.GregorianCalendar();
            //tempDate.Year = year;
            //tempDate.Month = month;
            //tempDate.Day = day;
            return tempDate.Ticks;
            // Aroush}}
        }
	}
}