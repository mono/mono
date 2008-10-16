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
using Field = Lucene.Net.Documents.Field;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Index
{
	[TestFixture]
	public class TestSegmentReader
	{
		private RAMDirectory dir = new RAMDirectory();
		private Document testDoc = new Document();
		private SegmentReader reader = null;
		
		//TODO: Setup the reader w/ multiple documents
        [TestFixtureSetUp]
		protected virtual void  SetUp()
		{
			
			try
			{
				DocHelper.SetupDoc(testDoc);
				DocHelper.WriteDoc(dir, testDoc);
				reader = new SegmentReader(new SegmentInfo("test", 1, dir));
			}
			catch (System.IO.IOException e)
			{
				
			}
		}
		
        [TestFixtureTearDown]
		protected virtual void  TearDown()
		{
			
		}
		
        [Test]
		public virtual void  Test()
		{
			Assert.IsTrue(dir != null);
			Assert.IsTrue(reader != null);
			Assert.IsTrue(DocHelper.nameValues.Count > 0);
			Assert.IsTrue(DocHelper.NumFields(testDoc) == 6);
		}
		
        [Test]
		public virtual void  TestDocument()
		{
			try
			{
				Assert.IsTrue(reader.NumDocs() == 1);
				Assert.IsTrue(reader.MaxDoc() >= 1);
				Document result = reader.Document(0);
				Assert.IsTrue(result != null);
				//There are 2 unstored fields on the document that are not preserved across writing
				Assert.IsTrue(DocHelper.NumFields(result) == DocHelper.NumFields(testDoc) - 2);
				
                foreach (Field field in result.Fields())
                {
                    Assert.IsTrue(field != null);
                    Assert.IsTrue(DocHelper.nameValues.Contains(field.Name()));
                }
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		
        [Test]
		public virtual void  TestDelete()
		{
			Document docToDelete = new Document();
			DocHelper.SetupDoc(docToDelete);
			DocHelper.WriteDoc(dir, "seg-to-delete", docToDelete);
			try
			{
				SegmentReader deleteReader = new SegmentReader(new SegmentInfo("seg-to-delete", 1, dir));
				Assert.IsTrue(deleteReader != null);
				Assert.IsTrue(deleteReader.NumDocs() == 1);
				deleteReader.Delete(0);
				Assert.IsTrue(deleteReader.IsDeleted(0) == true);
				Assert.IsTrue(deleteReader.HasDeletions() == true);
				Assert.IsTrue(deleteReader.NumDocs() == 0);
				try
				{
					Document test = deleteReader.Document(0);
					Assert.IsTrue(false);
				}
				catch (System.ArgumentException e)
				{
					Assert.IsTrue(true);
				}
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		
        [Test]
		public virtual void  TestGetFieldNameVariations()
		{
			try
			{
				System.Collections.ICollection result = reader.GetFieldNames();
				Assert.IsTrue(result != null);
				Assert.IsTrue(result.Count == 7);
				for (System.Collections.IEnumerator iter = result.GetEnumerator(); iter.MoveNext(); )
				{
                    System.Collections.DictionaryEntry fi = (System.Collections.DictionaryEntry) iter.Current;
                    System.String s = fi.Key.ToString();
					//System.out.println("Name: " + s);
					Assert.IsTrue(DocHelper.nameValues.Contains(s) == true || s.Equals(""));
				}
				result = reader.GetFieldNames(true);
				Assert.IsTrue(result != null);
				//      System.out.println("Size: " + result.size());
				Assert.IsTrue(result.Count == 5);
				for (System.Collections.IEnumerator iter = result.GetEnumerator(); iter.MoveNext(); )
				{
                    System.Collections.DictionaryEntry fi = (System.Collections.DictionaryEntry) iter.Current;
                    System.String s = fi.Key.ToString();
                    Assert.IsTrue(DocHelper.nameValues.Contains(s) == true || s.Equals(""));
				}
				
				result = reader.GetFieldNames(false);
				Assert.IsTrue(result != null);
				Assert.IsTrue(result.Count == 2);
				//Get all indexed fields that are storing term vectors
				result = reader.GetIndexedFieldNames(true);
				Assert.IsTrue(result != null);
				Assert.IsTrue(result.Count == 2);
				
				result = reader.GetIndexedFieldNames(false);
				Assert.IsTrue(result != null);
				Assert.IsTrue(result.Count == 3);
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		
        [Test]
		public virtual void  TestTerms()
		{
			try
			{
				TermEnum terms = reader.Terms();
				Assert.IsTrue(terms != null);
				while (terms.Next() == true)
				{
					Term term = terms.Term();
					Assert.IsTrue(term != null);
					//System.out.println("Term: " + term);
					System.String fieldValue = (System.String) DocHelper.nameValues[term.Field()];
					Assert.IsTrue(fieldValue.IndexOf(term.Text()) != - 1);
				}
				
				TermDocs termDocs = reader.TermDocs();
				Assert.IsTrue(termDocs != null);
				termDocs.Seek(new Term(DocHelper.TEXT_FIELD_1_KEY, "Field"));
				Assert.IsTrue(termDocs.Next() == true);
				
				TermPositions positions = reader.TermPositions();
				positions.Seek(new Term(DocHelper.TEXT_FIELD_1_KEY, "Field"));
				Assert.IsTrue(positions != null);
				Assert.IsTrue(positions.Doc() == 0);
				Assert.IsTrue(positions.NextPosition() >= 0);
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		
        [Test]
		public virtual void  TestNorms()
		{
			//TODO: Not sure how these work/should be tested
			/*
			try {
			byte [] norms = reader.norms(DocHelper.TEXT_FIELD_1_KEY);
			System.out.println("Norms: " + norms);
			Assert.IsTrue(norms != null);
			} catch (IOException e) {
			e.printStackTrace();
			Assert.IsTrue(false);
			}*/
		}
		
        [Test]
		public virtual void  TestTermVectors()
		{
			try
			{
				TermFreqVector result = reader.GetTermFreqVector(0, DocHelper.TEXT_FIELD_2_KEY);
				Assert.IsTrue(result != null);
				System.String[] terms = result.GetTerms();
				int[] freqs = result.GetTermFrequencies();
				Assert.IsTrue(terms != null && terms.Length == 3 && freqs != null && freqs.Length == 3);
				for (int i = 0; i < terms.Length; i++)
				{
					System.String term = terms[i];
					int freq = freqs[i];
					Assert.IsTrue(DocHelper.FIELD_2_TEXT.IndexOf(term) != - 1);
					Assert.IsTrue(freq > 0);
				}
				
				TermFreqVector[] results = reader.GetTermFreqVectors(0);
				Assert.IsTrue(results != null);
				Assert.IsTrue(results.Length == 2);
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
	}
}