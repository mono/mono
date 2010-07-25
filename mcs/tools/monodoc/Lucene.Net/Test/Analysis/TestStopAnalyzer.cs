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
namespace Lucene.Net.Analysis
{
	[TestFixture]
	public class TestStopAnalyzer
	{
		private StopAnalyzer stop = new StopAnalyzer();
		
		private System.Collections.Hashtable inValidTokens = new System.Collections.Hashtable();
		
        [TestFixtureSetUp]
		protected virtual void  SetUp()
		{
			for (int i = 0; i < StopAnalyzer.ENGLISH_STOP_WORDS.Length; i++)
			{
				inValidTokens.Add(StopAnalyzer.ENGLISH_STOP_WORDS[i], StopAnalyzer.ENGLISH_STOP_WORDS[i]);
			}
		}
		
        [Test]
		public virtual void  TestDefaults()
		{
			Assert.IsTrue(stop != null);
			System.IO.StringReader reader = new System.IO.StringReader("This is a test of the english stop analyzer");
			TokenStream stream = stop.TokenStream("test", reader);
			Assert.IsTrue(stream != null);
			Token token = null;
			try
			{
				while ((token = stream.Next()) != null)
				{
					Assert.IsTrue(inValidTokens.Contains(token.TermText()) == false);
				}
			}
			catch (System.IO.IOException e)
			{
				Assert.IsTrue(false);
			}
		}
		
        [Test]
		public virtual void  TestStopList()
		{
			System.Collections.Hashtable stopWordsSet = new System.Collections.Hashtable();
			stopWordsSet.Add("good", "good");
			stopWordsSet.Add("test", "test");
			stopWordsSet.Add("analyzer", "analyzer");

            // {{Aroush  how can we copy 'stopWordsSet' to 'System.String[]'?
            System.String[] arrStopWordsSet = new System.String[3];
            arrStopWordsSet[0] = "good";
            arrStopWordsSet[1] = "test";
            arrStopWordsSet[2] = "analyzer";
            // Aroush}}

			StopAnalyzer newStop = new StopAnalyzer(arrStopWordsSet);
			System.IO.StringReader reader = new System.IO.StringReader("This is a good test of the english stop analyzer");
			TokenStream stream = newStop.TokenStream("test", reader);
			Assert.IsTrue(stream != null);
			Token token = null;
			try
			{
				while ((token = stream.Next()) != null)
				{
					System.String text = token.TermText();
					Assert.IsTrue(stopWordsSet.Contains(text) == false);
				}
			}
			catch (System.IO.IOException e)
			{
				Assert.IsTrue(false);
			}
		}
	}
}