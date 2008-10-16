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
using Document = Lucene.Net.Documents.Document;
using Directory = Lucene.Net.Store.Directory;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Index
{
	[TestFixture]
	public class TestSegmentMerger
	{
		//The variables for the new merged segment
		private Directory mergedDir = new RAMDirectory();
		private System.String mergedSegment = "test";
		//First segment to be merged
		private Directory merge1Dir = new RAMDirectory();
		private Document doc1 = new Document();
		private System.String merge1Segment = "test-1";
		private SegmentReader reader1 = null;
		//Second Segment to be merged
		private Directory merge2Dir = new RAMDirectory();
		private Document doc2 = new Document();
		private System.String merge2Segment = "test-2";
		private SegmentReader reader2 = null;
		
        [TestFixtureSetUp]
		protected virtual void  SetUp()
		{
			DocHelper.SetupDoc(doc1);
			DocHelper.WriteDoc(merge1Dir, merge1Segment, doc1);
			DocHelper.SetupDoc(doc2);
			DocHelper.WriteDoc(merge2Dir, merge2Segment, doc2);
			try
			{
				reader1 = new SegmentReader(new SegmentInfo(merge1Segment, 1, merge1Dir));
				reader2 = new SegmentReader(new SegmentInfo(merge2Segment, 1, merge2Dir));
			}
			catch (System.IO.IOException e)
			{
                System.Console.Error.WriteLine(e.StackTrace);
			}
		}
		
        [TestFixtureTearDown]
		protected virtual void  TearDown()
		{
			
		}
		
        [Test]
		public virtual void  Test()
		{
            Assert.IsTrue(mergedDir != null);
			Assert.IsTrue(merge1Dir != null);
			Assert.IsTrue(merge2Dir != null);
			Assert.IsTrue(reader1 != null);
			Assert.IsTrue(reader2 != null);
		}
		
        [Test]
		public virtual void  TestMerge()
		{
			//System.out.println("----------------TestMerge------------------");
			SegmentMerger merger = new SegmentMerger(mergedDir, mergedSegment, false);
			merger.Add(reader1);
			merger.Add(reader2);
			try
			{
				int docsMerged = merger.Merge();
				merger.CloseReaders();
				Assert.IsTrue(docsMerged == 2);
				//Should be able to open a new SegmentReader against the new directory
				SegmentReader mergedReader = new SegmentReader(new SegmentInfo(mergedSegment, docsMerged, mergedDir));
				Assert.IsTrue(mergedReader != null);
				Assert.IsTrue(mergedReader.NumDocs() == 2);
				Document newDoc1 = mergedReader.Document(0);
				Assert.IsTrue(newDoc1 != null);
				//There are 2 unstored fields on the document
				Assert.IsTrue(DocHelper.NumFields(newDoc1) == DocHelper.NumFields(doc1) - 2);
				Document newDoc2 = mergedReader.Document(1);
				Assert.IsTrue(newDoc2 != null);
				Assert.IsTrue(DocHelper.NumFields(newDoc2) == DocHelper.NumFields(doc2) - 2);
				
				TermDocs termDocs = mergedReader.TermDocs(new Term(DocHelper.TEXT_FIELD_2_KEY, "Field"));
				Assert.IsTrue(termDocs != null);
				Assert.IsTrue(termDocs.Next() == true);
				
				System.Collections.ICollection stored = mergedReader.GetIndexedFieldNames(true);
				Assert.IsTrue(stored != null);
				//System.out.println("stored size: " + stored.size());
				Assert.IsTrue(stored.Count == 2);
				
				TermFreqVector vector = mergedReader.GetTermFreqVector(0, DocHelper.TEXT_FIELD_2_KEY);
				Assert.IsTrue(vector != null);
				System.String[] terms = vector.GetTerms();
				Assert.IsTrue(terms != null);
				//System.out.println("Terms size: " + terms.length);
				Assert.IsTrue(terms.Length == 3);
				int[] freqs = vector.GetTermFrequencies();
				Assert.IsTrue(freqs != null);
				//System.out.println("Freqs size: " + freqs.length);
				
				for (int i = 0; i < terms.Length; i++)
				{
					System.String term = terms[i];
					int freq = freqs[i];
					//System.out.println("Term: " + term + " Freq: " + freq);
					Assert.IsTrue(DocHelper.FIELD_2_TEXT.IndexOf(term) != - 1);
					Assert.IsTrue(DocHelper.FIELD_2_FREQS[i] == freq);
				}
			}
			catch (System.IO.IOException e)
			{
                System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
			//System.out.println("---------------------end TestMerge-------------------");
		}
	}
}