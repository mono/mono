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
using OutputStream = Lucene.Net.Store.OutputStream;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using RAMOutputStream = Lucene.Net.Store.RAMOutputStream;
namespace Lucene.Net.Index
{
	
	//import org.cnlp.utils.properties.ResourceBundleHelper;
	[TestFixture]
	public class TestFieldInfos
	{
		
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
			//Positive test of FieldInfos
			Assert.IsTrue(testDoc != null);
			FieldInfos fieldInfos = new FieldInfos();
			fieldInfos.Add(testDoc);
			//Since the complement is stored as well in the fields map
			Assert.IsTrue(fieldInfos.Size() == 7); //this is 7 b/c we are using the no-arg constructor
			RAMDirectory dir = new RAMDirectory();
			System.String name = "testFile";
			OutputStream output = dir.CreateFile(name);
			Assert.IsTrue(output != null);
			//Use a RAMOutputStream
			
			try
			{
				fieldInfos.Write(output);
				output.Close();
				Assert.IsTrue(output.Length() > 0);
				FieldInfos readIn = new FieldInfos(dir, name);
				Assert.IsTrue(fieldInfos.Size() == readIn.Size());
				FieldInfo info = readIn.FieldInfo("textField1");
				Assert.IsTrue(info != null);
				Assert.IsTrue(info.storeTermVector == false);
				
				info = readIn.FieldInfo("textField2");
				Assert.IsTrue(info != null);
				Assert.IsTrue(info.storeTermVector == true);
				
				dir.Close();
			}
			catch (System.IO.IOException e)
			{
				Assert.IsTrue(false);
			}
		}
	}
}