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
using Analyzer = Lucene.Net.Analysis.Analyzer;
using Token = Lucene.Net.Analysis.Token;
using TokenStream = Lucene.Net.Analysis.TokenStream;
using WhitespaceAnalyzer = Lucene.Net.Analysis.WhitespaceAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using NUnit.Framework;
namespace Lucene.Net.Search
{
	
	/// <summary> Term position unit test.
	/// 
	/// </summary>
	/// <author>  Doug Cutting
	/// </author>
	/// <version>  $Revision: 1.4 $
	/// </version>
	[TestFixture]
    public class TestPositionIncrement
	{
		private class AnonymousClassAnalyzer : Analyzer
		{
			public AnonymousClassAnalyzer(TestPositionIncrement enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private class AnonymousClassTokenStream : TokenStream
			{
				public AnonymousClassTokenStream(AnonymousClassAnalyzer enclosingInstance)
				{
					InitBlock(enclosingInstance);
				}
				private void  InitBlock(AnonymousClassAnalyzer enclosingInstance)
				{
					this.enclosingInstance = enclosingInstance;
				}
				private AnonymousClassAnalyzer enclosingInstance;
				public AnonymousClassAnalyzer Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				private System.String[] TOKENS = new System.String[]{"1", "2", "3", "4", "5"};
				private int[] INCREMENTS = new int[]{1, 2, 1, 0, 1};
				private int i = 0;
				
				public override Token Next()
				{
					if (i == TOKENS.Length)
						return null;
					Token t = new Token(TOKENS[i], i, i);
					t.SetPositionIncrement(INCREMENTS[i]);
					i++;
					return t;
				}
			}
			private void  InitBlock(TestPositionIncrement enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private TestPositionIncrement enclosingInstance;
			public TestPositionIncrement Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
			{
				return new AnonymousClassTokenStream(this);
			}
		}
		
        [Test]
		public virtual void  TestSetPosition()
		{
			Analyzer analyzer = new AnonymousClassAnalyzer(this);
			RAMDirectory store = new RAMDirectory();
			IndexWriter writer = new IndexWriter(store, analyzer, true);
			Document d = new Document();
			d.Add(Field.Text("Field", "bogus"));
			writer.AddDocument(d);
			writer.Optimize();
			writer.Close();
			
			IndexSearcher searcher = new IndexSearcher(store);
			PhraseQuery q;
			Hits hits;
			
			q = new PhraseQuery();
			q.Add(new Term("Field", "1"));
			q.Add(new Term("Field", "2"));
			hits = searcher.Search(q);
			Assert.AreEqual(0, hits.Length());
			
			q = new PhraseQuery();
			q.Add(new Term("Field", "2"));
			q.Add(new Term("Field", "3"));
			hits = searcher.Search(q);
			Assert.AreEqual(1, hits.Length());
			
			q = new PhraseQuery();
			q.Add(new Term("Field", "3"));
			q.Add(new Term("Field", "4"));
			hits = searcher.Search(q);
			Assert.AreEqual(0, hits.Length());
			
			q = new PhraseQuery();
			q.Add(new Term("Field", "2"));
			q.Add(new Term("Field", "4"));
			hits = searcher.Search(q);
			Assert.AreEqual(1, hits.Length());
			
			q = new PhraseQuery();
			q.Add(new Term("Field", "3"));
			q.Add(new Term("Field", "5"));
			hits = searcher.Search(q);
			Assert.AreEqual(1, hits.Length());
			
			q = new PhraseQuery();
			q.Add(new Term("Field", "4"));
			q.Add(new Term("Field", "5"));
			hits = searcher.Search(q);
			Assert.AreEqual(1, hits.Length());
			
			q = new PhraseQuery();
			q.Add(new Term("Field", "2"));
			q.Add(new Term("Field", "5"));
			hits = searcher.Search(q);
			Assert.AreEqual(0, hits.Length());
		}
		
		/// <summary> Basic analyzer behavior should be to keep sequential terms in one
		/// increment from one another.
		/// </summary>
		[Test]
        public virtual void  TestIncrementingPositions()
		{
			Analyzer analyzer = new WhitespaceAnalyzer();
			TokenStream ts = analyzer.TokenStream("Field", new System.IO.StringReader("one two three four five"));
			
			while (true)
			{
				Token token = ts.Next();
				if (token == null)
					break;
				Assert.AreEqual(1, token.GetPositionIncrement(), token.TermText());
			}
		}
	}
}