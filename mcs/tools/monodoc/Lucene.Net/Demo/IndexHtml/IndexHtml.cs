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
using IndexReader = Lucene.Net.Index.IndexReader;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using TermEnum = Lucene.Net.Index.TermEnum;
namespace Lucene.Net.Demo
{
	
	class IndexHTML
	{
		private static bool deleting = false; // true during deletion pass
		private static IndexReader reader; // existing index
		private static IndexWriter writer; // new index being built
		private static TermEnum uidIter; // document id iterator
		
		[STAThread]
		public static void  Main(System.String[] argv)
		{
			try
			{
				System.String index = "index";
				bool create = false;
				System.IO.FileInfo root = null;
				
				System.String usage = "IndexHTML [-create] [-index <index>] <root_directory>";
				
				if (argv.Length == 0)
				{
					System.Console.Error.WriteLine("Usage: " + usage);
					return ;
				}
				
				for (int i = 0; i < argv.Length; i++)
				{
					if (argv[i].Equals("-index"))
					{
						// parse -index option
						index = argv[++i];
					}
					else if (argv[i].Equals("-create"))
					{
						// parse -create option
						create = true;
					}
					else if (i != argv.Length - 1)
					{
						System.Console.Error.WriteLine("Usage: " + usage);
						return ;
					}
					else
						root = new System.IO.FileInfo(argv[i]);
				}
				
				System.DateTime start = System.DateTime.Now;
				
				if (!create)
				{
					// delete stale docs
					deleting = true;
					IndexDocs(root, index, create);
				}
				
				writer = new IndexWriter(index, new StandardAnalyzer(), create);
				writer.maxFieldLength = 1000000;
				
				IndexDocs(root, index, create); // add new docs
				
				System.Console.Out.WriteLine("Optimizing index...");
				writer.Optimize();
				writer.Close();
				
				System.DateTime end = System.DateTime.Now;
				
				System.Console.Out.Write(end.Ticks - start.Ticks);
				System.Console.Out.WriteLine(" total milliseconds");
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
			}
		}
		
		/* Walk directory hierarchy in uid order, while keeping uid iterator from
		/* existing index in sync.  Mismatches indicate one of: (a) old documents to
		/* be deleted; (b) unchanged documents, to be left alone; or (c) new
		/* documents, to be indexed.
		*/
		
		private static void  IndexDocs(System.IO.FileInfo file, System.String index, bool create)
		{
			if (!create)
			{
				// incrementally update
				
				reader = IndexReader.Open(index); // open existing index
				uidIter = reader.Terms(new Term("uid", "")); // init uid iterator
				
				IndexDocs(file);
				
				if (deleting)
				{
					// delete rest of stale docs
					while (uidIter.Term() != null && (System.Object) uidIter.Term().Field() == (System.Object) "uid")
					{
						System.Console.Out.WriteLine("deleting " + HTMLDocument.UID2URL(uidIter.Term().Text()));
						reader.Delete(uidIter.Term());
						uidIter.Next();
					}
					deleting = false;
				}
				
				uidIter.Close(); // close uid iterator
				reader.Close(); // close existing index
			}
			// don't have exisiting
			else
				IndexDocs(file);
		}
		
		private static void  IndexDocs(System.IO.FileInfo file)
		{
			if (System.IO.Directory.Exists(file.FullName))
			{
				// if a directory
				System.String[] files = System.IO.Directory.GetFileSystemEntries(file.FullName); // list its files
				System.Array.Sort(files); // sort the files
				for (int i = 0; i < files.Length; i++)
				// recursively index them
                    IndexDocs(new System.IO.FileInfo(files[i]));
			}
			else if (file.FullName.EndsWith(".html") || file.FullName.EndsWith(".htm") || file.FullName.EndsWith(".txt"))
			{
				// index .txt files
				
				if (uidIter != null)
				{
					System.String uid = HTMLDocument.UID(file); // construct uid for doc
					
					while (uidIter.Term() != null && (System.Object) uidIter.Term().Field() == (System.Object) "uid" && String.CompareOrdinal(uidIter.Term().Text(), uid) < 0)
					{
						if (deleting)
						{
							// delete stale docs
							System.Console.Out.WriteLine("deleting " + HTMLDocument.UID2URL(uidIter.Term().Text()));
							reader.Delete(uidIter.Term());
						}
						uidIter.Next();
					}
					if (uidIter.Term() != null && (System.Object) uidIter.Term().Field() == (System.Object) "uid" && String.CompareOrdinal(uidIter.Term().Text(), uid) == 0)
					{
						uidIter.Next(); // keep matching docs
					}
					else if (!deleting)
					{
						// add new docs
						Document doc = HTMLDocument.Document(file);
						System.Console.Out.WriteLine("adding " + doc.Get("url"));
						writer.AddDocument(doc);
					}
				}
				else
				{
					// creating a new index
					Document doc = HTMLDocument.Document(file);
					System.Console.Out.WriteLine("adding " + doc.Get("url"));
					writer.AddDocument(doc); // add docs unconditionally
				}
			}
		}
	}
}