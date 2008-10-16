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
using Analyzer = Lucene.Net.Analysis.Analyzer;
using StandardAnalyzer = Lucene.Net.Analysis.Standard.StandardAnalyzer;
using Document = Lucene.Net.Documents.Document;
using QueryParser = Lucene.Net.QueryParsers.QueryParser;
using Hits = Lucene.Net.Search.Hits;
using IndexSearcher = Lucene.Net.Search.IndexSearcher;
using Query = Lucene.Net.Search.Query;
using Searcher = Lucene.Net.Search.Searcher;
namespace Lucene.Net.Demo
{
	
	class SearchFiles
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			try
			{
				Searcher searcher = new IndexSearcher(@"index");
				Analyzer analyzer = new StandardAnalyzer();
				
				System.IO.StreamReader in_Renamed = new System.IO.StreamReader(new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).CurrentEncoding);
				while (true)
				{
					System.Console.Out.Write("Query: ");
					System.String line = in_Renamed.ReadLine();
					
					if (line.Length == - 1)
						break;
					
					Query query = QueryParser.Parse(line, "contents", analyzer);
					System.Console.Out.WriteLine("Searching for: " + query.ToString("contents"));
					
					Hits hits = searcher.Search(query);
					System.Console.Out.WriteLine(hits.Length() + " total matching documents");
					
					int HITS_PER_PAGE = 10;
					for (int start = 0; start < hits.Length(); start += HITS_PER_PAGE)
					{
						int end = System.Math.Min(hits.Length(), start + HITS_PER_PAGE);
						for (int i = start; i < end; i++)
						{
							Document doc = hits.Doc(i);
							System.String path = doc.Get("path");
							if (path != null)
							{
								System.Console.Out.WriteLine(i + ". " + path);
							}
							else
							{
								System.String url = doc.Get("url");
								if (url != null)
								{
									System.Console.Out.WriteLine(i + ". " + url);
									System.Console.Out.WriteLine("   - " + doc.Get("title"));
								}
								else
								{
									System.Console.Out.WriteLine(i + ". " + "No path nor URL for this document");
								}
							}
						}
						
						if (hits.Length() > end)
						{
							System.Console.Out.Write("more (y/n) ? ");
							line = in_Renamed.ReadLine();
							if (line.Length == 0 || line[0] == 'n')
								break;
						}
					}
				}
				searcher.Close();
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
			}
		}
	}
}