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
using WhitespaceAnalyzer = Lucene.Net.Analysis.WhitespaceAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using Directory = Lucene.Net.Store.Directory;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Index
{
	
	/// <author>  goller
	/// </author>
	[TestFixture]
    public class TestSegmentTermEnum
	{
		internal Directory dir = new RAMDirectory();
		
        [Test]
		public virtual void  TestTermEnum()
		{
			IndexWriter writer = null;
			
			try
			{
				writer = new IndexWriter(dir, new WhitespaceAnalyzer(), true);
				
				// add 100 documents with term : aaa
				// add 100 documents with terms: aaa bbb
				// Therefore, term 'aaa' has document frequency of 200 and term 'bbb' 100
				for (int i = 0; i < 100; i++)
				{
					AddDoc(writer, "aaa");
					AddDoc(writer, "aaa bbb");
				}
				
				writer.Close();
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
			}
			
			try
			{
				// verify document frequency of terms in an unoptimized index
				VerifyDocFreq();
				
				// merge segments by optimizing the index
				writer = new IndexWriter(dir, new WhitespaceAnalyzer(), false);
				writer.Optimize();
				writer.Close();
				
				// verify document frequency of terms in an optimized index
				VerifyDocFreq();
			}
			catch (System.IO.IOException e2)
			{
				System.Console.Error.WriteLine(e2.StackTrace);
			}
		}
		
		private void  VerifyDocFreq()
		{
			IndexReader reader = IndexReader.Open(dir);
			TermEnum termEnum = null;
			
			// create enumeration of all terms
			termEnum = reader.Terms();
			// go to the first term (aaa)
			termEnum.Next();
			// assert that term is 'aaa'
			Assert.AreEqual("aaa", termEnum.Term().Text());
			Assert.AreEqual(200, termEnum.DocFreq());
			// go to the second term (bbb)
			termEnum.Next();
			// assert that term is 'bbb'
			Assert.AreEqual("bbb", termEnum.Term().Text());
			Assert.AreEqual(100, termEnum.DocFreq());
			
			termEnum.Close();
			
			
			// create enumeration of terms after term 'aaa', including 'aaa'
			termEnum = reader.Terms(new Term("content", "aaa"));
			// assert that term is 'aaa'
			Assert.AreEqual("aaa", termEnum.Term().Text());
			Assert.AreEqual(200, termEnum.DocFreq());
			// go to term 'bbb'
			termEnum.Next();
			// assert that term is 'bbb'
			Assert.AreEqual("bbb", termEnum.Term().Text());
			Assert.AreEqual(100, termEnum.DocFreq());
			
			termEnum.Close();
		}
		
		private void  AddDoc(IndexWriter writer, System.String value_Renamed)
		{
			Document doc = new Document();
			doc.Add(Field.UnStored("content", value_Renamed));
			
			try
			{
				writer.AddDocument(doc);
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
			}
		}
	}
}