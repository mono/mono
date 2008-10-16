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
	public class TestTermVectorsReader
	{
		private TermVectorsWriter writer = null;
		//Must be lexicographically sorted, will do in setup, versus trying to maintain here
		private System.String[] testFields = new System.String[]{"f1", "f2", "f3"};
		private System.String[] testTerms = new System.String[]{"this", "is", "a", "test"};
		private RAMDirectory dir = new RAMDirectory();
		private System.String seg = "testSegment";
		private FieldInfos fieldInfos = new FieldInfos();
		
        [TestFixtureSetUp]
		protected virtual void  SetUp()
		{
			for (int i = 0; i < testFields.Length; i++)
			{
				fieldInfos.Add(testFields[i], true, true);
			}
			
			try
			{
				System.Array.Sort(testTerms);
				for (int j = 0; j < 5; j++)
				{
					writer = new TermVectorsWriter(dir, seg, fieldInfos);
					writer.OpenDocument();
					
					for (int k = 0; k < testFields.Length; k++)
					{
						writer.OpenField(testFields[k]);
						for (int i = 0; i < testTerms.Length; i++)
						{
							writer.AddTerm(testTerms[i], i);
						}
						writer.CloseField();
					}
					writer.CloseDocument();
					writer.Close();
				}
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		
        [TestFixtureTearDown]
		protected virtual void  TearDown()
		{
			
		}
		
        [Test]
		public virtual void  Test()
		{
			//Check to see the files were created properly in setup
			Assert.IsTrue(writer.IsDocumentOpen() == false);
			Assert.IsTrue(dir.FileExists(seg + TermVectorsWriter.TVD_EXTENSION));
			Assert.IsTrue(dir.FileExists(seg + TermVectorsWriter.TVX_EXTENSION));
		}
		
        [Test]
		public virtual void  TestReader()
		{
			try
			{
				TermVectorsReader reader = new TermVectorsReader(dir, seg, fieldInfos);
				Assert.IsTrue(reader != null);
				TermFreqVector vector = reader.Get(0, testFields[0]);
				Assert.IsTrue(vector != null);
				System.String[] terms = vector.GetTerms();
				Assert.IsTrue(terms != null);
				Assert.IsTrue(terms.Length == testTerms.Length);
				for (int i = 0; i < terms.Length; i++)
				{
					System.String term = terms[i];
					//System.out.println("Term: " + term);
					Assert.IsTrue(term.Equals(testTerms[i]));
				}
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		
		/// <summary> Make sure exceptions and bad params are handled appropriately</summary>
		[Test]
        public virtual void  TestBadParams()
		{
			try
			{
				TermVectorsReader reader = new TermVectorsReader(dir, seg, fieldInfos);
				Assert.IsTrue(reader != null);
				//Bad document number, good Field number
				TermFreqVector vector = reader.Get(50, testFields[0]);
				Assert.IsTrue(vector == null);
			}
			catch (System.Exception e)
			{
				Assert.IsTrue(false);
			}
			try
			{
				TermVectorsReader reader = new TermVectorsReader(dir, seg, fieldInfos);
				Assert.IsTrue(reader != null);
				//good document number, bad Field number
				TermFreqVector vector = reader.Get(0, "f50");
				Assert.IsTrue(vector == null);
			}
			catch (System.Exception e)
			{
				Assert.IsTrue(false);
			}
		}
	}
}