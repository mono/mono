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
using SimpleAnalyzer = Lucene.Net.Analysis.SimpleAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexReader = Lucene.Net.Index.IndexReader;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Search
{
	
	/// <summary>Document boost unit test.
	/// 
	/// </summary>
	/// <author>  Doug Cutting
	/// </author>
	/// <version>  $Revision: 1.2 $
	/// </version>
	[TestFixture]
    public class TestSetNorm
	{
		private class AnonymousClassHitCollector : HitCollector
		{
			public AnonymousClassHitCollector(float[] scores, TestSetNorm enclosingInstance)
			{
				InitBlock(scores, enclosingInstance);
			}
			private void  InitBlock(float[] scores, TestSetNorm enclosingInstance)
			{
				this.scores = scores;
				this.enclosingInstance = enclosingInstance;
			}
			private float[] scores;
			private TestSetNorm enclosingInstance;
			public TestSetNorm Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override void  Collect(int doc, float score)
			{
				scores[doc] = score;
			}
		}
		
        [Test]
		public virtual void  TestSetNorm_()
		{
			RAMDirectory store = new RAMDirectory();
			IndexWriter writer = new IndexWriter(store, new SimpleAnalyzer(), true);
			
			// add the same document four times
			Field f1 = Field.Text("Field", "word");
			Document d1 = new Document();
			d1.Add(f1);
			writer.AddDocument(d1);
			writer.AddDocument(d1);
			writer.AddDocument(d1);
			writer.AddDocument(d1);
			writer.Close();
			
			// reset the boost of each instance of this document
			IndexReader reader = IndexReader.Open(store);
			reader.SetNorm(0, "Field", 1.0f);
			reader.SetNorm(1, "Field", 2.0f);
			reader.SetNorm(2, "Field", 4.0f);
			reader.SetNorm(3, "Field", 16.0f);
			reader.Close();
			
			// check that searches are ordered by this boost
			float[] scores = new float[4];
			
			new IndexSearcher(store).Search(new TermQuery(new Term("Field", "word")), new AnonymousClassHitCollector(scores, this));
			
			float lastScore = 0.0f;
			
			for (int i = 0; i < 4; i++)
			{
				Assert.IsTrue(scores[i] > lastScore);
				lastScore = scores[i];
			}
		}
	}
}