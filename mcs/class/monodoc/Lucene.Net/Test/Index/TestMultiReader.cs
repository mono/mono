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
	public class TestMultiReader
	{
		private Directory dir = new RAMDirectory();
		private Document doc1 = new Document();
		private Document doc2 = new Document();
		private SegmentReader reader1;
		private SegmentReader reader2;
		private SegmentReader[] readers = new SegmentReader[2];
		private SegmentInfos sis = new SegmentInfos();
		
        [TestFixtureSetUp]
		protected virtual void  SetUp()
		{
			DocHelper.SetupDoc(doc1);
			DocHelper.SetupDoc(doc2);
			DocHelper.WriteDoc(dir, "seg-1", doc1);
			DocHelper.WriteDoc(dir, "seg-2", doc2);
			
			try
			{
				sis.Write(dir);
				reader1 = new SegmentReader(new SegmentInfo("seg-1", 1, dir));
				reader2 = new SegmentReader(new SegmentInfo("seg-2", 1, dir));
				readers[0] = reader1;
				readers[1] = reader2;
			}
			catch (System.IO.IOException e)
			{
                System.Console.Error.WriteLine(e.StackTrace);
			}
		}
		/*IndexWriter writer  = new IndexWriter(dir, new WhitespaceAnalyzer(), true);
		writer.addDocument(doc1);
		writer.addDocument(doc2);
		writer.close();*/
        [TestFixtureTearDown]
		protected virtual void  TearDown()
		{
			
		}
		
        [Test]
		public virtual void  Test()
		{
			Assert.IsTrue(dir != null);
			Assert.IsTrue(reader1 != null);
			Assert.IsTrue(reader2 != null);
			Assert.IsTrue(sis != null);
		}
		
        [Test]
		public virtual void  TestDocument()
		{
			try
			{
				sis.Read(dir);
				MultiReader reader = new MultiReader(dir, sis, false, readers);
				Assert.IsTrue(reader != null);
				Document newDoc1 = reader.Document(0);
				Assert.IsTrue(newDoc1 != null);
				Assert.IsTrue(DocHelper.NumFields(newDoc1) == DocHelper.NumFields(doc1) - 2);
				Document newDoc2 = reader.Document(1);
				Assert.IsTrue(newDoc2 != null);
				Assert.IsTrue(DocHelper.NumFields(newDoc2) == DocHelper.NumFields(doc2) - 2);
				TermFreqVector vector = reader.GetTermFreqVector(0, DocHelper.TEXT_FIELD_2_KEY);
				Assert.IsTrue(vector != null);
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		
        [Test]
		public virtual void  TestTermVectors()
		{
			try
			{
				MultiReader reader = new MultiReader(dir, sis, false, readers);
				Assert.IsTrue(reader != null);
			}
			catch (System.IO.IOException e)
			{
                System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
	}
}