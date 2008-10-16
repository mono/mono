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
using Analyzer = Lucene.Net.Analysis.Analyzer;
using WhitespaceAnalyzer = Lucene.Net.Analysis.WhitespaceAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using Similarity = Lucene.Net.Search.Similarity;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Index
{
	[TestFixture]
	public class TestDocumentWriter
	{
		private RAMDirectory dir = new RAMDirectory();
		private Document testDoc = new Document();
		
		[TestFixtureSetUp]
		protected virtual void  SetUp()
		{
			DocHelper.SetupDoc(testDoc);
		}
		
        [TestFixtureTearDown]
		protected virtual void  TearDown()
		{
			
		}
		
        [Test]
		public virtual void  Test()
		{
			Assert.IsTrue(dir != null);
		}
		
        [Test]
		public virtual void  TestAddDocument()
		{
			Analyzer analyzer = new WhitespaceAnalyzer();
			Similarity similarity = Similarity.GetDefault();
			DocumentWriter writer = new DocumentWriter(dir, analyzer, similarity, 50);
			Assert.IsTrue(writer != null);
			try
			{
				writer.AddDocument("test", testDoc);
				//After adding the document, we should be able to read it back in
				SegmentReader reader = new SegmentReader(new SegmentInfo("test", 1, dir));
				Assert.IsTrue(reader != null);
				Document doc = reader.Document(0);
				Assert.IsTrue(doc != null);
				
				//System.out.println("Document: " + doc);
				Field[] fields = doc.GetFields("textField2");
				Assert.IsTrue(fields != null && fields.Length == 1);
				Assert.IsTrue(fields[0].StringValue().Equals(DocHelper.FIELD_2_TEXT));
				Assert.IsTrue(fields[0].IsTermVectorStored() == true);
				
				fields = doc.GetFields("textField1");
				Assert.IsTrue(fields != null && fields.Length == 1);
				Assert.IsTrue(fields[0].StringValue().Equals(DocHelper.FIELD_1_TEXT));
				Assert.IsTrue(fields[0].IsTermVectorStored() == false);
				
				fields = doc.GetFields("keyField");
				Assert.IsTrue(fields != null && fields.Length == 1);
				Assert.IsTrue(fields[0].StringValue().Equals(DocHelper.KEYWORD_TEXT));
			}
			catch (System.IO.IOException e)
			{
                System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
	}
}