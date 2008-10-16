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
using StandardAnalyzer = Lucene.Net.Analysis.Standard.StandardAnalyzer;
using IndexReader = Lucene.Net.Index.IndexReader;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Directory = Lucene.Net.Store.Directory;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Search
{
	[TestFixture]
	public class TestCachingWrapperFilter
	{
        [Test]
		public virtual void  TestCachingWorks()
		{
			Directory dir = new RAMDirectory();
			IndexWriter writer = new IndexWriter(dir, new StandardAnalyzer(), true);
			writer.Close();
			
			IndexReader reader = IndexReader.Open(dir);
			
			MockFilter filter = new MockFilter();
			CachingWrapperFilter cacher = new CachingWrapperFilter(filter);
			
			// first time, nested filter is called
			cacher.Bits(reader);
			Assert.IsTrue(filter.WasCalled(), "first time");
			
			// second time, nested filter should not be called
			filter.Clear();
			cacher.Bits(reader);
			Assert.IsFalse(filter.WasCalled(), "second time");
			
			reader.Close();
		}
	}
}