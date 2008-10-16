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
	/// <version>  $Revision: 1.4 $
	/// </version>
	[TestFixture]
    public class TestDocBoost
	{
		private class AnonymousClassHitCollector:HitCollector
		{
			public AnonymousClassHitCollector(float[] scores, TestDocBoost enclosingInstance)
			{
				InitBlock(scores, enclosingInstance);
			}
			private void  InitBlock(float[] scores, TestDocBoost enclosingInstance)
			{
				this.scores = scores;
				this.enclosingInstance = enclosingInstance;
			}
			private float[] scores;
			private TestDocBoost enclosingInstance;
			public TestDocBoost Enclosing_Instance
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
		public virtual void  TestDocBoost_()
		{
			RAMDirectory store = new RAMDirectory();
			IndexWriter writer = new IndexWriter(store, new SimpleAnalyzer(), true);
			
			Field f1 = Field.Text("Field", "word");
			Field f2 = Field.Text("Field", "word");
			f2.SetBoost(2.0f);
			
			Document d1 = new Document();
			Document d2 = new Document();
			Document d3 = new Document();
			Document d4 = new Document();
			d3.SetBoost(3.0f);
			d4.SetBoost(2.0f);
			
			d1.Add(f1); // boost = 1
			d2.Add(f2); // boost = 2
			d3.Add(f1); // boost = 3
			d4.Add(f2); // boost = 4
			
			writer.AddDocument(d1);
			writer.AddDocument(d2);
			writer.AddDocument(d3);
			writer.AddDocument(d4);
			writer.Optimize();
			writer.Close();
			
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