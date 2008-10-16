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
using Lucene.Net;
using Lucene.Net.Analysis;
namespace Lucene.Net.Analysis
{
	[TestFixture]
	public class TestAnalyzers
	{
		public virtual void  AssertAnalyzesTo(Analyzer a, System.String input, System.String[] output)
		{
			TokenStream ts = a.TokenStream("dummy", new System.IO.StringReader(input));
			for (int i = 0; i < output.Length; i++)
			{
				Token t = ts.Next();
				Assert.IsNotNull(t);
				Assert.AreEqual(t.TermText(), output[i]);
			}
			Assert.IsNull(ts.Next());
			ts.Close();
		}
		
        [Test]
		public virtual void  TestSimple()
		{
			Analyzer a = new SimpleAnalyzer();
			AssertAnalyzesTo(a, "foo bar FOO BAR", new System.String[]{"foo", "bar", "foo", "bar"});
			AssertAnalyzesTo(a, "foo      bar .  FOO <> BAR", new System.String[]{"foo", "bar", "foo", "bar"});
			AssertAnalyzesTo(a, "foo.bar.FOO.BAR", new System.String[]{"foo", "bar", "foo", "bar"});
			AssertAnalyzesTo(a, "U.S.A.", new System.String[]{"u", "s", "a"});
			AssertAnalyzesTo(a, "C++", new System.String[]{"c"});
			AssertAnalyzesTo(a, "B2B", new System.String[]{"b", "b"});
			AssertAnalyzesTo(a, "2B", new System.String[]{"b"});
			AssertAnalyzesTo(a, "\"QUOTED\" word", new System.String[]{"quoted", "word"});
		}
		
        [Test]
		public virtual void  TestNull()
		{
			Analyzer a = new WhitespaceAnalyzer();
			AssertAnalyzesTo(a, "foo bar FOO BAR", new System.String[]{"foo", "bar", "FOO", "BAR"});
			AssertAnalyzesTo(a, "foo      bar .  FOO <> BAR", new System.String[]{"foo", "bar", ".", "FOO", "<>", "BAR"});
			AssertAnalyzesTo(a, "foo.bar.FOO.BAR", new System.String[]{"foo.bar.FOO.BAR"});
			AssertAnalyzesTo(a, "U.S.A.", new System.String[]{"U.S.A."});
			AssertAnalyzesTo(a, "C++", new System.String[]{"C++"});
			AssertAnalyzesTo(a, "B2B", new System.String[]{"B2B"});
			AssertAnalyzesTo(a, "2B", new System.String[]{"2B"});
			AssertAnalyzesTo(a, "\"QUOTED\" word", new System.String[]{"\"QUOTED\"", "word"});
		}
		
        [Test]
		public virtual void  TestStop()
		{
			Analyzer a = new StopAnalyzer();
			AssertAnalyzesTo(a, "foo bar FOO BAR", new System.String[]{"foo", "bar", "foo", "bar"});
			AssertAnalyzesTo(a, "foo a bar such FOO THESE BAR", new System.String[]{"foo", "bar", "foo", "bar"});
		}
	}
}