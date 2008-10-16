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
namespace Lucene.Net.Search
{
	[TestFixture]
	public class TestQueryTermVector
	{
        [TestFixtureSetUp]
		protected virtual void  SetUp()
		{
		}
		
        [TestFixtureTearDown]
		protected virtual void  TearDown()
		{
			
		}
		
        [Test]
		public virtual void  TestConstructor()
		{
			System.String[] queryTerm = new System.String[]{"foo", "bar", "foo", "again", "foo", "bar", "go", "go", "go"};
			//Items are sorted lexicographically
			System.String[] gold = new System.String[]{"again", "bar", "foo", "go"};
			int[] goldFreqs = new int[]{1, 2, 3, 3};
			QueryTermVector result = new QueryTermVector(queryTerm);
			Assert.IsTrue(result != null);
			System.String[] terms = result.GetTerms();
			Assert.IsTrue(terms.Length == 4);
			int[] freq = result.GetTermFrequencies();
			Assert.IsTrue(freq.Length == 4);
			CheckGold(terms, gold, freq, goldFreqs);
			result = new QueryTermVector(null);
			Assert.IsTrue(result.GetTerms().Length == 0);
			
			result = new QueryTermVector("foo bar foo again foo bar go go go", new WhitespaceAnalyzer());
			Assert.IsTrue(result != null);
			terms = result.GetTerms();
			Assert.IsTrue(terms.Length == 4);
			freq = result.GetTermFrequencies();
			Assert.IsTrue(freq.Length == 4);
			CheckGold(terms, gold, freq, goldFreqs);
		}
		
		private void  CheckGold(System.String[] terms, System.String[] gold, int[] freq, int[] goldFreqs)
		{
			for (int i = 0; i < terms.Length; i++)
			{
				Assert.IsTrue(terms[i].Equals(gold[i]));
				Assert.IsTrue(freq[i] == goldFreqs[i]);
			}
		}
	}
}