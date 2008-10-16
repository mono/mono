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
using StandardAnalyzer = Lucene.Net.Analysis.Standard.StandardAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using Hits = Lucene.Net.Search.Hits;
using IndexSearcher = Lucene.Net.Search.IndexSearcher;
using Searcher = Lucene.Net.Search.Searcher;
using TermQuery = Lucene.Net.Search.TermQuery;
using Directory = Lucene.Net.Store.Directory;
using FSDirectory = Lucene.Net.Store.FSDirectory;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Index
{
	[TestFixture]
	public class TestFilterIndexReader
	{
		private class TestReader : FilterIndexReader
		{
			
			/// <summary>Filter that only permits terms containing 'e'.</summary>
			private class TestTermEnum : FilterTermEnum
			{
				public TestTermEnum(TermEnum termEnum) : base(termEnum)
				{
				}
				
				/// <summary>Scan for terms containing the letter 'e'.</summary>
				public override bool Next()
				{
					while (in_Renamed.Next())
					{
						if (in_Renamed.Term().Text().IndexOf((System.Char) 'e') != - 1)
							return true;
					}
					return false;
				}
			}
			
			/// <summary>Filter that only returns odd numbered documents. </summary>
			private class TestTermPositions : FilterTermPositions
			{
				public TestTermPositions(TermPositions in_Renamed):base(in_Renamed)
				{
				}
				
				/// <summary>Scan for odd numbered documents. </summary>
				public override bool Next()
				{
					while (in_Renamed.Next())
					{
						if ((in_Renamed.Doc() % 2) == 1)
							return true;
					}
					return false;
				}
			}
			
			public TestReader(IndexReader reader) : base(reader)
			{
			}
			
			/// <summary>Filter terms with TestTermEnum. </summary>
			public override TermEnum Terms()
			{
				return new TestTermEnum(in_Renamed.Terms());
			}
			
			/// <summary>Filter positions with TestTermPositions. </summary>
			public override TermPositions TermPositions()
			{
				return new TestTermPositions(in_Renamed.TermPositions());
			}
		}
		
		
		/// <summary>Main for running test case by itself. </summary>
		[STAThread]
		public static void  Main(System.String[] args)
		{
		}
		
		/// <summary> Tests the IndexReader.getFieldNames implementation</summary>
		/// <throws>  Exception on error </throws>
		[Test]
		public virtual void  TestFilterIndexReader_()
		{
			RAMDirectory directory = new RAMDirectory();
			IndexWriter writer = new IndexWriter(directory, new WhitespaceAnalyzer(), true);
			
			Document d1 = new Document();
			d1.Add(Field.Text("default", "one two"));
			writer.AddDocument(d1);
			
			Document d2 = new Document();
			d2.Add(Field.Text("default", "one three"));
			writer.AddDocument(d2);
			
			Document d3 = new Document();
			d3.Add(Field.Text("default", "two four"));
			writer.AddDocument(d3);
			
			writer.Close();
			
			IndexReader reader = new TestReader(IndexReader.Open(directory));
			
			TermEnum terms = reader.Terms();
			while (terms.Next())
			{
				Assert.IsTrue(terms.Term().Text().IndexOf((System.Char) 'e') != - 1);
			}
			terms.Close();
			
			TermPositions positions = reader.TermPositions(new Term("default", "one"));
			while (positions.Next())
			{
				Assert.IsTrue((positions.Doc() % 2) == 1);
			}
			
			reader.Close();
		}
	}
}