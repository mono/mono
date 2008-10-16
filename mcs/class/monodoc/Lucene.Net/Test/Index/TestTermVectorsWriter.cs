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
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Index
{
	[TestFixture]
	public class TestTermVectorsWriter
	{
		private System.String[] testTerms = new System.String[]{"this", "is", "a", "test"};
		private System.String[] testFields = new System.String[]{"f1", "f2", "f3"};
		private int[][] positions = new int[3][];
		private RAMDirectory dir = new RAMDirectory();
		private System.String seg = "testSegment";
		private FieldInfos fieldInfos = new FieldInfos();
		
        [TestFixtureSetUp]
		protected virtual void  SetUp()
		{
            positions = new int[testTerms.Length][];
			
			for (int i = 0; i < testFields.Length; i++)
			{
				fieldInfos.Add(testFields[i], true, true);
			}
			
			
			for (int i = 0; i < testTerms.Length; i++)
			{
				positions[i] = new int[5];
				for (int j = 0; j < positions[i].Length; j++)
				{
					positions[i][j] = i * 100;
				}
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
			Assert.IsTrue(positions != null);
		}
		
		/*public void testWriteNoPositions() {
		try {
		TermVectorsWriter writer = new TermVectorsWriter(dir, seg, 50);
		writer.openDocument();
		Assert.IsTrue(writer.isDocumentOpen() == true);
		writer.openField(0);
		Assert.IsTrue(writer.isFieldOpen() == true);
		for (int i = 0; i < testTerms.length; i++) {
		writer.addTerm(testTerms[i], i);
		}
		writer.closeField();
		
		writer.closeDocument();
		writer.close();
		Assert.IsTrue(writer.isDocumentOpen() == false);
		//Check to see the files were created
		Assert.IsTrue(dir.fileExists(seg + TermVectorsWriter.TVD_EXTENSION));
		Assert.IsTrue(dir.fileExists(seg + TermVectorsWriter.TVX_EXTENSION));
		//Now read it back in
		TermVectorsReader reader = new TermVectorsReader(dir, seg);
		Assert.IsTrue(reader != null);
		CheckTermVector(reader, 0, 0);
		} catch (IOException e) {
		e.printStackTrace();
		Assert.IsTrue(false);
		}
		}  */
		
        [Test]
		public virtual void  TestWriter()
		{
			try
			{
				TermVectorsWriter writer = new TermVectorsWriter(dir, seg, fieldInfos);
				writer.OpenDocument();
				Assert.IsTrue(writer.IsDocumentOpen() == true);
				WriteField(writer, testFields[0]);
				writer.CloseDocument();
				writer.Close();
				Assert.IsTrue(writer.IsDocumentOpen() == false);
				//Check to see the files were created
				Assert.IsTrue(dir.FileExists(seg + TermVectorsWriter.TVD_EXTENSION));
				Assert.IsTrue(dir.FileExists(seg + TermVectorsWriter.TVX_EXTENSION));
				//Now read it back in
				TermVectorsReader reader = new TermVectorsReader(dir, seg, fieldInfos);
				Assert.IsTrue(reader != null);
				CheckTermVector(reader, 0, testFields[0]);
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		private void  CheckTermVector(TermVectorsReader reader, int docNum, System.String field)
		{
			TermFreqVector vector = reader.Get(docNum, field);
			Assert.IsTrue(vector != null);
			System.String[] terms = vector.GetTerms();
			Assert.IsTrue(terms != null);
			Assert.IsTrue(terms.Length == testTerms.Length);
			for (int i = 0; i < terms.Length; i++)
			{
				System.String term = terms[i];
				Assert.IsTrue(term.Equals(testTerms[i]));
			}
		}
		
		/// <summary> Test one document, multiple fields</summary>
		[Test]
		public virtual void  TestMultipleFields()
		{
			try
			{
				TermVectorsWriter writer = new TermVectorsWriter(dir, seg, fieldInfos);
				WriteDocument(writer, testFields.Length);
				
				writer.Close();
				
				Assert.IsTrue(writer.IsDocumentOpen() == false);
				//Check to see the files were created
				Assert.IsTrue(dir.FileExists(seg + TermVectorsWriter.TVD_EXTENSION));
				Assert.IsTrue(dir.FileExists(seg + TermVectorsWriter.TVX_EXTENSION));
				//Now read it back in
				TermVectorsReader reader = new TermVectorsReader(dir, seg, fieldInfos);
				Assert.IsTrue(reader != null);
				
				for (int j = 0; j < testFields.Length; j++)
				{
					CheckTermVector(reader, 0, testFields[j]);
				}
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		
		private void  WriteDocument(TermVectorsWriter writer, int numFields)
		{
			writer.OpenDocument();
			Assert.IsTrue(writer.IsDocumentOpen() == true);
			
			for (int j = 0; j < numFields; j++)
			{
				WriteField(writer, testFields[j]);
			}
			writer.CloseDocument();
			Assert.IsTrue(writer.IsDocumentOpen() == false);
		}
		
		/// <summary> </summary>
		/// <param name="writer">The writer to write to
		/// </param>
		/// <param name="j">The Field number
		/// </param>
		/// <throws>  IOException </throws>
		private void  WriteField(TermVectorsWriter writer, System.String f)
		{
			writer.OpenField(f);
			Assert.IsTrue(writer.IsFieldOpen() == true);
			for (int i = 0; i < testTerms.Length; i++)
			{
				writer.AddTerm(testTerms[i], i);
			}
			writer.CloseField();
		}
		
		[Test]
		public virtual void  TestMultipleDocuments()
		{
			
			try
			{
				TermVectorsWriter writer = new TermVectorsWriter(dir, seg, fieldInfos);
				Assert.IsTrue(writer != null);
				for (int i = 0; i < 10; i++)
				{
					WriteDocument(writer, testFields.Length);
				}
				writer.Close();
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
			//Do some arbitrary tests
			try
			{
				TermVectorsReader reader = new TermVectorsReader(dir, seg, fieldInfos);
				for (int i = 0; i < 10; i++)
				{
					Assert.IsTrue(reader != null);
					CheckTermVector(reader, 5, testFields[0]);
					CheckTermVector(reader, 2, testFields[2]);
				}
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
	}
}