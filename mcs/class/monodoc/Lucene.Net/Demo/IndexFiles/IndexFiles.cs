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
using IndexWriter = Lucene.Net.Index.IndexWriter;
namespace Lucene.Net.Demo
{
	
	class IndexFiles
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String usage = typeof(IndexFiles) + " <root_directory>";
			if (args.Length == 0)
			{
				System.Console.Error.WriteLine("Usage: " + usage);
				System.Environment.Exit(1);
			}
			
			System.DateTime start = System.DateTime.Now;
			try
			{
				IndexWriter writer = new IndexWriter("index", new StandardAnalyzer(), true);
                IndexDocs(writer, new System.IO.FileInfo(args[0]));
				
				writer.Optimize();
				writer.Close();
				
				System.DateTime end = System.DateTime.Now;
				
				System.Console.Out.Write(end.Ticks - start.Ticks);
				System.Console.Out.WriteLine(" total milliseconds");
			}
			catch (System.IO.IOException e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
			}
		}
		
		public static void  IndexDocs(IndexWriter writer, System.IO.FileInfo file)
		{
			if (System.IO.Directory.Exists(file.FullName))
			{
				System.String[] files = System.IO.Directory.GetFileSystemEntries(file.FullName);
				// an IO error could occur
				if (files != null)
				{
					for (int i = 0; i < files.Length; i++)
					{
						IndexDocs(writer, new System.IO.FileInfo(files[i]));
					}
				}
			}
			else
			{
				System.Console.Out.WriteLine("adding " + file);
				try
				{
					writer.AddDocument(FileDocument.Document(file));
				}
				// at least on windows, some temporary files raise this exception with an "access denied" message
				// checking if the file can be read doesn't help
				catch (System.IO.FileNotFoundException fnfe)
				{
					;
				}
			}
		}
	}
}