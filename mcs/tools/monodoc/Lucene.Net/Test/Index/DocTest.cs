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
using SimpleAnalyzer = Lucene.Net.Analysis.SimpleAnalyzer;
//using FileDocument = Lucene.Net.Demo.FileDocument;
using Document = Lucene.Net.Documents.Document;
using Similarity = Lucene.Net.Search.Similarity;
using Directory = Lucene.Net.Store.Directory;
using FSDirectory = Lucene.Net.Store.FSDirectory;
namespace Lucene.Net.Index
{
	
	// FIXME: OG: remove hard-coded file names
	class DocTest
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			try
			{
				Directory directory = FSDirectory.GetDirectory("test", true);
				directory.Close();
				
				IndexDoc("one", "test.txt");
				PrintSegment("one");
				IndexDoc("two", "test2.txt");
				PrintSegment("two");
				
				Merge("one", "two", "merge");
				PrintSegment("merge");
				
				Merge("one", "two", "merge2");
				PrintSegment("merge2");
				
				Merge("merge", "merge2", "merge3");
				PrintSegment("merge3");
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
                System.Console.Error.WriteLine(e.StackTrace);
			}
		}
		
		public static void  IndexDoc(System.String segment, System.String fileName)
		{
			Directory directory = FSDirectory.GetDirectory("test", false);
			Analyzer analyzer = new SimpleAnalyzer();
			DocumentWriter writer = new DocumentWriter(directory, analyzer, Similarity.GetDefault(), 1000);
			
			System.IO.FileInfo file = new System.IO.FileInfo(fileName);
			Document doc = Lucene.Net.Demo.FileDocument.Document(file);
			
			writer.AddDocument(segment, doc);
			
			directory.Close();
		}
		
		internal static void  Merge(System.String seg1, System.String seg2, System.String merged)
		{
			Directory directory = FSDirectory.GetDirectory("test", false);
			
			SegmentReader r1 = new SegmentReader(new SegmentInfo(seg1, 1, directory));
			SegmentReader r2 = new SegmentReader(new SegmentInfo(seg2, 1, directory));
			
			SegmentMerger merger = new SegmentMerger(directory, merged, false);
			merger.Add(r1);
			merger.Add(r2);
			merger.Merge();
			merger.CloseReaders();
			
			directory.Close();
		}
		
		internal static void  PrintSegment(System.String segment)
		{
			Directory directory = FSDirectory.GetDirectory("test", false);
			SegmentReader reader = new SegmentReader(new SegmentInfo(segment, 1, directory));
			
			for (int i = 0; i < reader.NumDocs(); i++)
			{
				System.Console.Out.WriteLine(reader.Document(i));
			}
			
			TermEnum tis = reader.Terms();
			while (tis.Next())
			{
				System.Console.Out.Write(tis.Term());
				System.Console.Out.WriteLine(" DF=" + tis.DocFreq());
				
				TermPositions positions = reader.TermPositions(tis.Term());
				try
				{
					while (positions.Next())
					{
						System.Console.Out.Write(" doc=" + positions.Doc());
						System.Console.Out.Write(" TF=" + positions.Freq());
						System.Console.Out.Write(" pos=");
						System.Console.Out.Write(positions.NextPosition());
						for (int j = 1; j < positions.Freq(); j++)
							System.Console.Out.Write("," + positions.NextPosition());
						System.Console.Out.WriteLine("");
					}
				}
				finally
				{
					positions.Close();
				}
			}
			tis.Close();
			reader.Close();
			directory.Close();
		}
	}
}