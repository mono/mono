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
////using NUnit.Framework.TestSuite;
////using NUnit.Framework.TestRunner;
using Analyzer = Lucene.Net.Analysis.Analyzer;
using SimpleAnalyzer = Lucene.Net.Analysis.SimpleAnalyzer;
using FileDocument = Lucene.Net.Demo.FileDocument;
using Document = Lucene.Net.Documents.Document;
using Similarity = Lucene.Net.Search.Similarity;
using Directory = Lucene.Net.Store.Directory;
using FSDirectory = Lucene.Net.Store.FSDirectory;
namespace Lucene.Net.Index
{
	
	
	/// <summary>JUnit adaptation of an older test case DocTest.</summary>
	/// <author>  dmitrys@earthlink.net
	/// </author>
	/// <version>  $Id: TestDoc.java,v 1.5 2004/04/20 19:33:35 goller Exp $
	/// </version>
	[TestFixture]
    public class TestDoc
	{
		
		/// <summary>Main for running test case by itself. </summary>
		[STAThread]
		public static void  Main(System.String[] args)
		{
		}
		
		
		private System.IO.FileInfo workDir;
		private System.IO.FileInfo indexDir;
		private System.Collections.ArrayList files;
		
		
		/// <summary>Set the test case. This test case needs
		/// a few text files created in the current working directory.
		/// </summary>
		[TestFixtureSetUp]
        public virtual void  SetUp()
		{
			workDir = new System.IO.FileInfo(System.Configuration.ConfigurationSettings.AppSettings.Get("tempDir") + "\\" + "TestDoc");
			System.IO.Directory.CreateDirectory(workDir.FullName);
			
			indexDir = new System.IO.FileInfo(workDir.FullName + "\\" + "testIndex");
			System.IO.Directory.CreateDirectory(indexDir.FullName);
			
			Directory directory = FSDirectory.GetDirectory(indexDir, true);
			directory.Close();
			
			files = new System.Collections.ArrayList();
			files.Add(CreateFile("test.txt", "This is the first test file"));
			
			files.Add(CreateFile("test2.txt", "This is the second test file"));
		}
		
		private System.IO.FileInfo CreateFile(System.String name, System.String text)
		{
			System.IO.StreamWriter fw = null;
			System.IO.StreamWriter pw = null;
			
			try
			{
				System.IO.FileInfo f = new System.IO.FileInfo(workDir.FullName + "\\" + name);
				bool tmpBool;
				if (System.IO.File.Exists(f.FullName))
					tmpBool = true;
				else
					tmpBool = System.IO.Directory.Exists(f.FullName);
				if (tmpBool)
				{
					bool tmpBool2;
					if (System.IO.File.Exists(f.FullName))
					{
						System.IO.File.Delete(f.FullName);
						tmpBool2 = true;
					}
					else if (System.IO.Directory.Exists(f.FullName))
					{
						System.IO.Directory.Delete(f.FullName);
						tmpBool2 = true;
					}
					else
						tmpBool2 = false;
					bool generatedAux = tmpBool2;
				}
				
				fw = new System.IO.StreamWriter(f.FullName, false, System.Text.Encoding.Default);
				pw = new System.IO.StreamWriter(fw.BaseStream, fw.Encoding);
				pw.WriteLine(text);
				return f;
			}
			finally
			{
				if (pw != null)
				{
					pw.Close();
				}
				if ((fw != null) && (fw.BaseStream.CanWrite))
					fw.Close();
			}
		}
		
		
		/// <summary>This test executes a number of merges and compares the contents of
		/// the segments created when using compound file or not using one.
		/// 
		/// TODO: the original test used to print the segment contents to System.out
		/// for visual validation. To have the same effect, a new method
		/// checkSegment(String name, ...) should be created that would
		/// assert various things about the segment.
		/// </summary>
		[Test]
		public virtual void  TestIndexAndMerge()
		{
			System.IO.StringWriter out_Renamed = new System.IO.StringWriter();
			
			Directory directory = FSDirectory.GetDirectory(indexDir, true);
			directory.Close();
			
			IndexDoc("one", "test.txt");
			PrintSegment(out_Renamed, "one");
			
			IndexDoc("two", "test2.txt");
			PrintSegment(out_Renamed, "two");
			
			Merge("one", "two", "merge", false);
			PrintSegment(out_Renamed, "merge");
			
			Merge("one", "two", "merge2", false);
			PrintSegment(out_Renamed, "merge2");
			
			Merge("merge", "merge2", "merge3", false);
			PrintSegment(out_Renamed, "merge3");
			
			out_Renamed.Close();
			System.String multiFileOutput = out_Renamed.GetStringBuilder().ToString();
			
			out_Renamed = new System.IO.StringWriter();
			
			directory = FSDirectory.GetDirectory(indexDir, true);
			directory.Close();
			
			IndexDoc("one", "test.txt");
			PrintSegment(out_Renamed, "one");
			
			IndexDoc("two", "test2.txt");
			PrintSegment(out_Renamed, "two");
			
			Merge("one", "two", "merge", true);
			PrintSegment(out_Renamed, "merge");
			
			Merge("one", "two", "merge2", true);
			PrintSegment(out_Renamed, "merge2");
			
			Merge("merge", "merge2", "merge3", true);
			PrintSegment(out_Renamed, "merge3");
			
			out_Renamed.Close();
			System.String singleFileOutput = out_Renamed.GetStringBuilder().ToString();
			
			Assert.AreEqual(multiFileOutput, singleFileOutput);
		}
		
		
		private void  IndexDoc(System.String segment, System.String fileName)
		{
			Directory directory = FSDirectory.GetDirectory(indexDir, false);
			Analyzer analyzer = new SimpleAnalyzer();
			DocumentWriter writer = new DocumentWriter(directory, analyzer, Similarity.GetDefault(), 1000);
			
			System.IO.FileInfo file = new System.IO.FileInfo(workDir.FullName + "\\" + fileName);
			Document doc = FileDocument.Document(file);
			
			writer.AddDocument(segment, doc);
			
			directory.Close();
		}
		
		
		private void  Merge(System.String seg1, System.String seg2, System.String merged, bool useCompoundFile)
		{
			Directory directory = FSDirectory.GetDirectory(indexDir, false);
			
			SegmentReader r1 = new SegmentReader(new SegmentInfo(seg1, 1, directory));
			SegmentReader r2 = new SegmentReader(new SegmentInfo(seg2, 1, directory));
			
			SegmentMerger merger = new SegmentMerger(directory, merged, useCompoundFile);
			
			merger.Add(r1);
			merger.Add(r2);
			merger.Merge();
			merger.CloseReaders();
			
			directory.Close();
		}
		
		
		private void  PrintSegment(System.IO.StringWriter out_Renamed, System.String segment)
		{
			Directory directory = FSDirectory.GetDirectory(indexDir, false);
			SegmentReader reader = new SegmentReader(new SegmentInfo(segment, 1, directory));
			
			for (int i = 0; i < reader.NumDocs(); i++)
			{
				out_Renamed.WriteLine(reader.Document(i));
			}
			
			TermEnum tis = reader.Terms();
			while (tis.Next())
			{
				out_Renamed.Write(tis.Term());
				out_Renamed.WriteLine(" DF=" + tis.DocFreq());
				
				TermPositions positions = reader.TermPositions(tis.Term());
				try
				{
					while (positions.Next())
					{
						out_Renamed.Write(" doc=" + positions.Doc());
						out_Renamed.Write(" TF=" + positions.Freq());
                        out_Renamed.Write(" pos=");
                        out_Renamed.Write(positions.NextPosition());
						for (int j = 1; j < positions.Freq(); j++)
							out_Renamed.Write("," + positions.NextPosition());
						out_Renamed.WriteLine("");
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