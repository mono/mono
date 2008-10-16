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
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using English = Lucene.Net.Util.English;
namespace Lucene.Net.Search
{
	[TestFixture]
	public class TestTermVectors
	{
		private IndexSearcher searcher;
		private RAMDirectory directory = new RAMDirectory();
		
        [TestFixtureSetUp]
		public virtual void  SetUp()
		{
			IndexWriter writer = new IndexWriter(directory, new SimpleAnalyzer(), true);
			//writer.setUseCompoundFile(true);
			//writer.infoStream = System.out;
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			for (int i = 0; i < 1000; i++)
			{
				Document doc = new Document();
				doc.Add(Field.Text("Field", English.IntToEnglish(i), true));
				writer.AddDocument(doc);
			}
			writer.Close();
			searcher = new IndexSearcher(directory);
		}
		
        [TestFixtureTearDown]
		protected virtual void  TearDown()
		{
			
		}
		
        [Test]
		public virtual void  Test()
		{
			Assert.IsTrue(searcher != null);
		}
		
        [Test]
		public virtual void  TestTermVectors_()
		{
			Query query = new TermQuery(new Term("Field", "seventy"));
			try
			{
				Hits hits = searcher.Search(query);
				Assert.AreEqual(100, hits.Length());
				
				for (int i = 0; i < hits.Length(); i++)
				{
					TermFreqVector[] vector = searcher.reader.GetTermFreqVectors(hits.Id(i));
					Assert.IsTrue(vector != null);
					Assert.IsTrue(vector.Length == 1);
					//Assert.IsTrue();
				}
				TermFreqVector[] vector2 = searcher.reader.GetTermFreqVectors(hits.Id(50));
				//System.out.println("Explain: " + searcher.explain(query, hits.id(50)));
				//System.out.println("Vector: " + vector[0].toString());
			}
			catch (System.IO.IOException e)
			{
				Assert.IsTrue(false);
			}
		}
		
        [Test]
		public virtual void  TestTermPositionVectors()
		{
			Query query = new TermQuery(new Term("Field", "fifty"));
			try
			{
				Hits hits = searcher.Search(query);
				Assert.AreEqual(100, hits.Length());
				
				for (int i = 0; i < hits.Length(); i++)
				{
					TermFreqVector[] vector = searcher.reader.GetTermFreqVectors(hits.Id(i));
					Assert.IsTrue(vector != null);
					Assert.IsTrue(vector.Length == 1);
					//Assert.IsTrue();
				}
			}
			catch (System.IO.IOException e)
			{
				Assert.IsTrue(false);
			}
		}
		
        [Test]
		public virtual void  TestKnownSetOfDocuments()
		{
			System.String[] termArray = new System.String[]{"eating", "chocolate", "in", "a", "computer", "lab", "grows", "old", "colored", "with", "an"};
			System.String test1 = "eating chocolate in a computer lab"; //6 terms
			System.String test2 = "computer in a computer lab"; //5 terms
			System.String test3 = "a chocolate lab grows old"; //5 terms
			System.String test4 = "eating chocolate with a chocolate lab in an old chocolate colored computer lab"; //13 terms
			System.Collections.IDictionary test4Map = new System.Collections.Hashtable();
			test4Map["chocolate"] = 3;
			test4Map["lab"] = 2;
			test4Map["eating"] = 1;
			test4Map["computer"] = 1;
			test4Map["with"] = 1;
			test4Map["a"] = 1;
			test4Map["colored"] = 1;
			test4Map["in"] = 1;
			test4Map["an"] = 1;
			test4Map["computer"] = 1;
			test4Map["old"] = 1;
			
			Document testDoc1 = new Document();
			SetupDoc(testDoc1, test1);
			Document testDoc2 = new Document();
			SetupDoc(testDoc2, test2);
			Document testDoc3 = new Document();
			SetupDoc(testDoc3, test3);
			Document testDoc4 = new Document();
			SetupDoc(testDoc4, test4);
			
			Directory dir = new RAMDirectory();
			
			try
			{
				IndexWriter writer = new IndexWriter(dir, new SimpleAnalyzer(), true);
				Assert.IsTrue(writer != null);
				writer.AddDocument(testDoc1);
				writer.AddDocument(testDoc2);
				writer.AddDocument(testDoc3);
				writer.AddDocument(testDoc4);
				writer.Close();
				IndexSearcher knownSearcher = new IndexSearcher(dir);
				TermEnum termEnum = knownSearcher.reader.Terms();
				TermDocs termDocs = knownSearcher.reader.TermDocs();
				//System.out.println("Terms: " + termEnum.size() + " Orig Len: " + termArray.length);
				
				Similarity sim = knownSearcher.GetSimilarity();
				while (termEnum.Next() == true)
				{
					Term term = termEnum.Term();
					//System.out.println("Term: " + term);
					termDocs.Seek(term);
					while (termDocs.Next())
					{
						int docId = termDocs.Doc();
						int freq = termDocs.Freq();
						//System.out.println("Doc Id: " + docId + " freq " + freq);
						TermFreqVector vector = knownSearcher.reader.GetTermFreqVector(docId, "Field");
						float tf = sim.Tf(freq);
						float idf = sim.Idf(term, knownSearcher);
						//float qNorm = sim.queryNorm()
						//This is fine since we don't have stop words
						float lNorm = sim.LengthNorm("Field", vector.GetTerms().Length);
						//float coord = sim.coord()
						//System.out.println("TF: " + tf + " IDF: " + idf + " LenNorm: " + lNorm);
						Assert.IsTrue(vector != null);
						System.String[] vTerms = vector.GetTerms();
						int[] freqs = vector.GetTermFrequencies();
						for (int i = 0; i < vTerms.Length; i++)
						{
							if (term.Text().Equals(vTerms[i]) == true)
							{
								Assert.IsTrue(freqs[i] == freq);
							}
						}
					}
					//System.out.println("--------");
				}
				Query query = new TermQuery(new Term("Field", "chocolate"));
				Hits hits = knownSearcher.Search(query);
				//doc 3 should be the first hit b/c it is the shortest match
				Assert.IsTrue(hits.Length() == 3);
				float score = hits.Score(0);
				/*System.out.println("Hit 0: " + hits.id(0) + " Score: " + hits.score(0) + " String: " + hits.doc(0).toString());
				System.out.println("Explain: " + knownSearcher.explain(query, hits.id(0)));
				System.out.println("Hit 1: " + hits.id(1) + " Score: " + hits.score(1) + " String: " + hits.doc(1).toString());
				System.out.println("Explain: " + knownSearcher.explain(query, hits.id(1)));
				System.out.println("Hit 2: " + hits.id(2) + " Score: " + hits.score(2) + " String: " +  hits.doc(2).toString());
				System.out.println("Explain: " + knownSearcher.explain(query, hits.id(2)));*/
				Assert.IsTrue(testDoc3.ToString().Equals(hits.Doc(0).ToString()));
				Assert.IsTrue(testDoc4.ToString().Equals(hits.Doc(1).ToString()));
				Assert.IsTrue(testDoc1.ToString().Equals(hits.Doc(2).ToString()));
				TermFreqVector vector2 = knownSearcher.reader.GetTermFreqVector(hits.Id(1), "Field");
				Assert.IsTrue(vector2 != null);
				//System.out.println("Vector: " + vector);
				System.String[] terms = vector2.GetTerms();
				int[] freqs2 = vector2.GetTermFrequencies();
				Assert.IsTrue(terms != null && terms.Length == 10);
				for (int i = 0; i < terms.Length; i++)
				{
                    System.String term = terms[i];
                    //System.out.println("Term: " + term);
                    int freq = freqs2[i];
                    Assert.IsTrue(test4.IndexOf(term) != - 1);
                    System.Int32 freqInt = (System.Int32) test4Map[term];
                    System.Object tmpFreqInt = test4Map[term];
                    Assert.IsTrue(tmpFreqInt != null);
                    Assert.IsTrue(freqInt == freq);
                }
				knownSearcher.Close();
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
				Assert.IsTrue(false);
			}
		}
		
		private void  SetupDoc(Document doc, System.String text)
		{
			doc.Add(Field.Text("Field", text, true));
			//System.out.println("Document: " + doc);
		}
	}
}