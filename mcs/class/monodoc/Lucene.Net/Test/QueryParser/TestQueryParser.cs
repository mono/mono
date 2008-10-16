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
using Analyzer = Lucene.Net.Analysis.Analyzer;
using LowerCaseTokenizer = Lucene.Net.Analysis.LowerCaseTokenizer;
using SimpleAnalyzer = Lucene.Net.Analysis.SimpleAnalyzer;
using Token = Lucene.Net.Analysis.Token;
using TokenFilter = Lucene.Net.Analysis.TokenFilter;
using TokenStream = Lucene.Net.Analysis.TokenStream;
using WhitespaceAnalyzer = Lucene.Net.Analysis.WhitespaceAnalyzer;
using StandardAnalyzer = Lucene.Net.Analysis.Standard.StandardAnalyzer;
using DateField = Lucene.Net.Documents.DateField;
using BooleanQuery = Lucene.Net.Search.BooleanQuery;
using FuzzyQuery = Lucene.Net.Search.FuzzyQuery;
using PhraseQuery = Lucene.Net.Search.PhraseQuery;
using PrefixQuery = Lucene.Net.Search.PrefixQuery;
using Query = Lucene.Net.Search.Query;
using RangeQuery = Lucene.Net.Search.RangeQuery;
using TermQuery = Lucene.Net.Search.TermQuery;
using WildcardQuery = Lucene.Net.Search.WildcardQuery;
namespace Lucene.Net.QueryParser
{
	
	/// <summary> Tests QueryParser.</summary>
	[TestFixture]
	public class TestQueryParser
	{
		
		public static Analyzer qpAnalyzer = new QPTestAnalyzer();
		
		public class QPTestFilter : TokenFilter
		{
			/// <summary> Filter which discards the token 'stop' and which expands the
			/// token 'phrase' into 'phrase1 phrase2'
			/// </summary>
			public QPTestFilter(TokenStream in_Renamed) : base(in_Renamed)
			{
			}
			
			internal bool inPhrase = false;
			internal int savedStart = 0, savedEnd = 0;
			
			public override Token Next()
			{
				if (inPhrase)
				{
					inPhrase = false;
					return new Token("phrase2", savedStart, savedEnd);
				}
				else
					for (Token token = input.Next(); token != null; token = input.Next())
					{
						if (token.TermText().Equals("phrase"))
						{
							inPhrase = true;
							savedStart = token.StartOffset();
							savedEnd = token.EndOffset();
							return new Token("phrase1", savedStart, savedEnd);
						}
						else if (!token.TermText().Equals("stop"))
							return token;
					}
				return null;
			}
		}
		
		public class QPTestAnalyzer : Analyzer
		{
			
			/// <summary>Filters LowerCaseTokenizer with StopFilter. </summary>
			public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
			{
				return new QPTestFilter(new LowerCaseTokenizer(reader));
			}
		}
		
		public class QPTestParser : QueryParsers.QueryParser
		{
			public QPTestParser(System.String f, Analyzer a) : base(f, a)
			{
			}
			
			protected /*internal*/ override Query GetFuzzyQuery(System.String field, System.String termStr)
			{
				throw new Lucene.Net.Analysis.Standard.ParseException("Fuzzy queries not allowed");
			}
			
			protected /*internal*/ override Query GetWildcardQuery(System.String field, System.String termStr)
			{
				throw new Lucene.Net.Analysis.Standard.ParseException("Wildcard queries not allowed");
			}
		}
		
		private int originalMaxClauses;
		
        [TestFixtureSetUp]
		public virtual void  SetUp()
		{
			originalMaxClauses = BooleanQuery.GetMaxClauseCount();
		}
		
		public virtual QueryParsers.QueryParser GetParser(Analyzer a)
		{
			if (a == null)
				a = new SimpleAnalyzer();
			QueryParsers.QueryParser qp = new QueryParsers.QueryParser("Field", a);
			qp.SetOperator(QueryParsers.QueryParser.DEFAULT_OPERATOR_OR);
			return qp;
		}
		
		public virtual Query GetQuery(System.String query, Analyzer a)
		{
			return GetParser(a).Parse(query);
		}
		
		public virtual void  AssertQueryEquals(System.String query, Analyzer a, System.String result)
		{
			Query q = GetQuery(query, a);
			System.String s = q.ToString("Field");
			if (!s.Equals(result))
			{
				Assert.Fail("Query /" + query + "/ yielded /" + s + "/, expecting /" + result + "/");
			}
		}
		
		public virtual void  AssertWildcardQueryEquals(System.String query, bool lowercase, System.String result)
		{
			QueryParsers.QueryParser qp = GetParser(null);
			qp.SetLowercaseWildcardTerms(lowercase);
			Query q = qp.Parse(query);
			System.String s = q.ToString("Field");
			if (!s.Equals(result))
			{
				Assert.Fail("WildcardQuery /" + query + "/ yielded /" + s + "/, expecting /" + result + "/");
			}
		}
		
		public virtual Query GetQueryDOA(System.String query, Analyzer a)
		{
			if (a == null)
				a = new SimpleAnalyzer();
			QueryParsers.QueryParser qp = new QueryParsers.QueryParser("Field", a);
			qp.SetOperator(QueryParsers.QueryParser.DEFAULT_OPERATOR_AND);
			return qp.Parse(query);
		}
		
		public virtual void  AssertQueryEqualsDOA(System.String query, Analyzer a, System.String result)
		{
			Query q = GetQueryDOA(query, a);
			System.String s = q.ToString("Field");
			if (!s.Equals(result))
			{
				Assert.Fail("Query /" + query + "/ yielded /" + s + "/, expecting /" + result + "/");
			}
		}
		
        [Test]
		public virtual void  TestSimple()
		{
			AssertQueryEquals("term term term", null, "term term term");
			AssertQueryEquals("türm term term", null, "türm term term");
			AssertQueryEquals("ümlaut", null, "ümlaut");
			
			AssertQueryEquals("a AND b", null, "+a +b");
			AssertQueryEquals("(a AND b)", null, "+a +b");
			AssertQueryEquals("c OR (a AND b)", null, "c (+a +b)");
			AssertQueryEquals("a AND NOT b", null, "+a -b");
			AssertQueryEquals("a AND -b", null, "+a -b");
			AssertQueryEquals("a AND !b", null, "+a -b");
			AssertQueryEquals("a && b", null, "+a +b");
			AssertQueryEquals("a && ! b", null, "+a -b");
			
			AssertQueryEquals("a OR b", null, "a b");
			AssertQueryEquals("a || b", null, "a b");
			AssertQueryEquals("a OR !b", null, "a -b");
			AssertQueryEquals("a OR ! b", null, "a -b");
			AssertQueryEquals("a OR -b", null, "a -b");
			
			AssertQueryEquals("+term -term term", null, "+term -term term");
			AssertQueryEquals("foo:term AND Field:anotherTerm", null, "+foo:term +anotherterm");
			AssertQueryEquals("term AND \"phrase phrase\"", null, "+term +\"phrase phrase\"");
			AssertQueryEquals("\"hello there\"", null, "\"hello there\"");
			Assert.IsTrue(GetQuery("a AND b", null) is BooleanQuery);
			Assert.IsTrue(GetQuery("hello", null) is TermQuery);
			Assert.IsTrue(GetQuery("\"hello there\"", null) is PhraseQuery);
			
			AssertQueryEquals("germ term^2.0", null, "germ term^2.0");
			AssertQueryEquals("(term)^2.0", null, "term^2.0");
			AssertQueryEquals("(germ term)^2.0", null, "(germ term)^2.0");
			AssertQueryEquals("term^2.0", null, "term^2.0");
			AssertQueryEquals("term^2", null, "term^2.0");
			AssertQueryEquals("\"germ term\"^2.0", null, "\"germ term\"^2.0");
			AssertQueryEquals("\"term germ\"^2", null, "\"term germ\"^2.0");
			
			AssertQueryEquals("(foo OR bar) AND (baz OR boo)", null, "+(foo bar) +(baz boo)");
			AssertQueryEquals("((a OR b) AND NOT c) OR d", null, "(+(a b) -c) d");
			AssertQueryEquals("+(apple \"steve jobs\") -(foo bar baz)", null, "+(apple \"steve jobs\") -(foo bar baz)");
			AssertQueryEquals("+title:(dog OR cat) -author:\"bob dole\"", null, "+(title:dog title:cat) -author:\"bob dole\"");
		}
		
        [Test]
		public virtual void  TestPunct()
		{
			Analyzer a = new WhitespaceAnalyzer();
			AssertQueryEquals("a&b", a, "a&b");
			AssertQueryEquals("a&&b", a, "a&&b");
			AssertQueryEquals(".NET", a, ".NET");
		}
		
        [Test]
		public virtual void  TestSlop()
		{
			AssertQueryEquals("\"term germ\"~2", null, "\"term germ\"~2");
			AssertQueryEquals("\"term germ\"~2 flork", null, "\"term germ\"~2 flork");
			AssertQueryEquals("\"term\"~2", null, "term");
			AssertQueryEquals("\" \"~2 germ", null, "germ");
			AssertQueryEquals("\"term germ\"~2^2", null, "\"term germ\"~2^2.0");
		}
		
        [Test]
		public virtual void  TestNumber()
		{
			// The numbers go away because SimpleAnalzyer ignores them
			AssertQueryEquals("3", null, "");
			AssertQueryEquals("term 1.0 1 2", null, "term");
			AssertQueryEquals("term term1 term2", null, "term term term");
			
			Analyzer a = new StandardAnalyzer();
			AssertQueryEquals("3", a, "3");
			AssertQueryEquals("term 1.0 1 2", a, "term 1.0 1 2");
			AssertQueryEquals("term term1 term2", a, "term term1 term2");
		}
		
        [Test]
		public virtual void  TestWildcard()
		{
			AssertQueryEquals("term*", null, "term*");
			AssertQueryEquals("term*^2", null, "term*^2.0");
			AssertQueryEquals("term~", null, "term~0.5");
            AssertQueryEquals("term~0.7", null, "term~0.7");
            AssertQueryEquals("term~^2", null, "term^2.0~0.5");
            AssertQueryEquals("term^2~", null, "term^2.0~0.5");
            AssertQueryEquals("term*germ", null, "term*germ");
            AssertQueryEquals("term*germ^3", null, "term*germ^3.0");
			
            Assert.IsTrue(GetQuery("term*", null) is PrefixQuery);
            Assert.IsTrue(GetQuery("term*^2", null) is PrefixQuery);
            Assert.IsTrue(GetQuery("term~", null) is FuzzyQuery);
            Assert.IsTrue(GetQuery("term~0.7", null) is FuzzyQuery);
            FuzzyQuery fq = (FuzzyQuery) GetQuery("term~0.7", null);
            Assert.AreEqual(0.7f, fq.GetMinSimilarity(), 0.1f);
            Assert.AreEqual(0, fq.GetPrefixLength());
            fq = (FuzzyQuery) GetQuery("term~", null);
            Assert.AreEqual(0.5f, fq.GetMinSimilarity(), 0.1f);
            Assert.AreEqual(0, fq.GetPrefixLength());
            try
            {
                GetQuery("term~1.1", null); // value > 1, throws exception
                Assert.Fail();
            }
            catch (Lucene.Net.QueryParsers.ParseException pe)
            {
                // expected exception
            }
            Assert.IsTrue(GetQuery("term*germ", null) is WildcardQuery);
			
			/* Tests to see that wild card terms are (or are not) properly
			* lower-cased with propery parser configuration
			*/
			// First prefix queries:
			AssertWildcardQueryEquals("term*", true, "term*");
			AssertWildcardQueryEquals("Term*", true, "term*");
			AssertWildcardQueryEquals("TERM*", true, "term*");
			AssertWildcardQueryEquals("term*", false, "term*");
			AssertWildcardQueryEquals("Term*", false, "Term*");
			AssertWildcardQueryEquals("TERM*", false, "TERM*");
			// Then 'full' wildcard queries:
			AssertWildcardQueryEquals("te?m", true, "te?m");
			AssertWildcardQueryEquals("Te?m", true, "te?m");
			AssertWildcardQueryEquals("TE?M", true, "te?m");
			AssertWildcardQueryEquals("Te?m*gerM", true, "te?m*germ");
			AssertWildcardQueryEquals("te?m", false, "te?m");
			AssertWildcardQueryEquals("Te?m", false, "Te?m");
			AssertWildcardQueryEquals("TE?M", false, "TE?M");
			AssertWildcardQueryEquals("Te?m*gerM", false, "Te?m*gerM");
		}
		
        [Test]
		public virtual void  TestQPA()
		{
			AssertQueryEquals("term term term", qpAnalyzer, "term term term");
			AssertQueryEquals("term +stop term", qpAnalyzer, "term term");
			AssertQueryEquals("term -stop term", qpAnalyzer, "term term");
			AssertQueryEquals("drop AND stop AND roll", qpAnalyzer, "+drop +roll");
			AssertQueryEquals("term phrase term", qpAnalyzer, "term \"phrase1 phrase2\" term");
			AssertQueryEquals("term AND NOT phrase term", qpAnalyzer, "+term -\"phrase1 phrase2\" term");
			AssertQueryEquals("stop", qpAnalyzer, "");
			Assert.IsTrue(GetQuery("term term term", qpAnalyzer) is BooleanQuery);
			Assert.IsTrue(GetQuery("term +stop", qpAnalyzer) is TermQuery);
		}
		
        [Test]
		public virtual void  TestRange()
		{
			AssertQueryEquals("[ a TO z]", null, "[a TO z]");
			Assert.IsTrue(GetQuery("[ a TO z]", null) is RangeQuery);
			AssertQueryEquals("[ a TO z ]", null, "[a TO z]");
			AssertQueryEquals("{ a TO z}", null, "{a TO z}");
			AssertQueryEquals("{ a TO z }", null, "{a TO z}");
			AssertQueryEquals("{ a TO z }^2.0", null, "{a TO z}^2.0");
			AssertQueryEquals("[ a TO z] OR bar", null, "[a TO z] bar");
			AssertQueryEquals("[ a TO z] AND bar", null, "+[a TO z] +bar");
			AssertQueryEquals("( bar blar { a TO z}) ", null, "bar blar {a TO z}");
			AssertQueryEquals("gack ( bar blar { a TO z}) ", null, "gack (bar blar {a TO z})");
		}
		
		public virtual System.String GetDate(System.String s)
		{
            return DateField.DateToString(DateTime.Parse(s));
		}
		
		public virtual System.String GetLocalizedDate(int year, int month, int day)
		{
            return new DateTime(year,month,day).ToShortDateString();
		}
		
        [Test]
		public virtual void  TestDateRange()
		{
			System.String startDate = GetLocalizedDate(2002, 1, 1);
			System.String endDate = GetLocalizedDate(2002, 1, 4);
			AssertQueryEquals("[ " + startDate + " TO " + endDate + "]", null, "[" + GetDate(startDate) + " TO " + GetDate(endDate) + "]");
			AssertQueryEquals("{  " + startDate + "    " + endDate + "   }", null, "{" + GetDate(startDate) + " TO " + GetDate(endDate) + "}");
		}
		
        [Test]
		public virtual void  TestEscaped()
		{
			Analyzer a = new WhitespaceAnalyzer();

            /*AssertQueryEquals("\\[brackets", a, "\\[brackets");
			AssertQueryEquals("\\[brackets", null, "brackets");
			AssertQueryEquals("\\\\", a, "\\\\");
			AssertQueryEquals("\\+blah", a, "\\+blah");
			AssertQueryEquals("\\(blah", a, "\\(blah");
			
			AssertQueryEquals("\\-blah", a, "\\-blah");
			AssertQueryEquals("\\!blah", a, "\\!blah");
			AssertQueryEquals("\\{blah", a, "\\{blah");
			AssertQueryEquals("\\}blah", a, "\\}blah");
			AssertQueryEquals("\\:blah", a, "\\:blah");
			AssertQueryEquals("\\^blah", a, "\\^blah");
			AssertQueryEquals("\\[blah", a, "\\[blah");
			AssertQueryEquals("\\]blah", a, "\\]blah");
			AssertQueryEquals("\\\"blah", a, "\\\"blah");
			AssertQueryEquals("\\(blah", a, "\\(blah");
			AssertQueryEquals("\\)blah", a, "\\)blah");
			AssertQueryEquals("\\~blah", a, "\\~blah");
			AssertQueryEquals("\\*blah", a, "\\*blah");
			AssertQueryEquals("\\?blah", a, "\\?blah");
			//AssertQueryEquals("foo \\&\\& bar", a, "foo \\&\\& bar");
			//AssertQueryEquals("foo \\|| bar", a, "foo \\|| bar");
			//AssertQueryEquals("foo \\AND bar", a, "foo \\AND bar");*/
			
			AssertQueryEquals("a\\-b:c", a, "a-b:c");
			AssertQueryEquals("a\\+b:c", a, "a+b:c");
			AssertQueryEquals("a\\:b:c", a, "a:b:c");
			AssertQueryEquals("a\\\\b:c", a, "a\\b:c");
			
			AssertQueryEquals("a:b\\-c", a, "a:b-c");
			AssertQueryEquals("a:b\\+c", a, "a:b+c");
			AssertQueryEquals("a:b\\:c", a, "a:b:c");
			AssertQueryEquals("a:b\\\\c", a, "a:b\\c");
			
			AssertQueryEquals("a:b\\-c*", a, "a:b-c*");
			AssertQueryEquals("a:b\\+c*", a, "a:b+c*");
			AssertQueryEquals("a:b\\:c*", a, "a:b:c*");
			
            AssertQueryEquals("a:b\\\\c*", a, "a:b\\c*");
			
			AssertQueryEquals("a:b\\-?c", a, "a:b-?c");
			AssertQueryEquals("a:b\\+?c", a, "a:b+?c");
			AssertQueryEquals("a:b\\:?c", a, "a:b:?c");
			
            AssertQueryEquals("a:b\\\\?c", a, "a:b\\?c");
			
			AssertQueryEquals("a:b\\-c~", a, "a:b-c~0.5");
			AssertQueryEquals("a:b\\+c~", a, "a:b+c~0.5");
			AssertQueryEquals("a:b\\:c~", a, "a:b:c~0.5");
			AssertQueryEquals("a:b\\\\c~", a, "a:b\\c~0.5");
			
			AssertQueryEquals("[ a\\- TO a\\+ ]", null, "[a- TO a+]");
			AssertQueryEquals("[ a\\: TO a\\~ ]", null, "[a: TO a~]");
			AssertQueryEquals("[ a\\\\ TO a\\* ]", null, "[a\\ TO a*]");
		}
		
        [Test]
		public virtual void  TestTabNewlineCarriageReturn()
		{
			AssertQueryEqualsDOA("+weltbank +worlbank", null, "+weltbank +worlbank");
			
			AssertQueryEqualsDOA("+weltbank\n+worlbank", null, "+weltbank +worlbank");
			AssertQueryEqualsDOA("weltbank \n+worlbank", null, "+weltbank +worlbank");
			AssertQueryEqualsDOA("weltbank \n +worlbank", null, "+weltbank +worlbank");
			
			AssertQueryEqualsDOA("+weltbank\r+worlbank", null, "+weltbank +worlbank");
			AssertQueryEqualsDOA("weltbank \r+worlbank", null, "+weltbank +worlbank");
			AssertQueryEqualsDOA("weltbank \r +worlbank", null, "+weltbank +worlbank");
			
			AssertQueryEqualsDOA("+weltbank\r\n+worlbank", null, "+weltbank +worlbank");
			AssertQueryEqualsDOA("weltbank \r\n+worlbank", null, "+weltbank +worlbank");
			AssertQueryEqualsDOA("weltbank \r\n +worlbank", null, "+weltbank +worlbank");
			AssertQueryEqualsDOA("weltbank \r \n +worlbank", null, "+weltbank +worlbank");
			
			AssertQueryEqualsDOA("+weltbank\t+worlbank", null, "+weltbank +worlbank");
			AssertQueryEqualsDOA("weltbank \t+worlbank", null, "+weltbank +worlbank");
			AssertQueryEqualsDOA("weltbank \t +worlbank", null, "+weltbank +worlbank");
		}
		
        [Test]
		public virtual void  TestSimpleDAO()
		{
			AssertQueryEqualsDOA("term term term", null, "+term +term +term");
			AssertQueryEqualsDOA("term +term term", null, "+term +term +term");
			AssertQueryEqualsDOA("term term +term", null, "+term +term +term");
			AssertQueryEqualsDOA("term +term +term", null, "+term +term +term");
			AssertQueryEqualsDOA("-term term term", null, "-term +term +term");
		}
		
        [Test]
		public virtual void  TestBoost()
		{
			StandardAnalyzer oneStopAnalyzer = new StandardAnalyzer(new System.String[]{"on"});
			QueryParsers.QueryParser qp = new QueryParsers.QueryParser("Field", oneStopAnalyzer);
			Query q = qp.Parse("on^1.0");
			Assert.IsNotNull(q);
			q = qp.Parse("\"hello\"^2.0");
			Assert.IsNotNull(q);
			Assert.AreEqual(q.GetBoost(), (float) 2.0, (float) 0.5);
			q = qp.Parse("hello^2.0");
			Assert.IsNotNull(q);
			Assert.AreEqual(q.GetBoost(), (float) 2.0, (float) 0.5);
			q = qp.Parse("\"on\"^1.0");
			Assert.IsNotNull(q);
			
			q = QueryParsers.QueryParser.Parse("the^3", "Field", new StandardAnalyzer());
			Assert.IsNotNull(q);
		}
		
        [Test]
		public virtual void  TestException()
		{
			try
			{
				AssertQueryEquals("\"some phrase", null, "abc");
				Assert.Fail("ParseException expected, not thrown");
			}
			catch (Lucene.Net.QueryParsers.ParseException expected)
			{
			}
		}
		
        [Test]
		public virtual void  TestCustomQueryParserWildcard()
		{
			try
			{
				new QPTestParser("contents", new WhitespaceAnalyzer()).Parse("a?t");
			}
			catch (Lucene.Net.Analysis.Standard.ParseException expected)
			{
				return ;
			}
			Assert.Fail("Wildcard queries should not be allowed");
		}
		
        [Test]
		public virtual void  TestCustomQueryParserFuzzy()
		{
			try
			{
				new QPTestParser("contents", new WhitespaceAnalyzer()).Parse("xunit~");
			}
			catch (Lucene.Net.Analysis.Standard.ParseException expected)
			{
				return ;
			}
			Assert.Fail("Fuzzy queries should not be allowed");
		}
		
        [Test]
		public virtual void  TestBooleanQuery()
		{
			BooleanQuery.SetMaxClauseCount(2);
			try
			{
				QueryParsers.QueryParser.Parse("one two three", "Field", new WhitespaceAnalyzer());
				Assert.Fail("ParseException expected due to too many boolean clauses");
			}
			catch (Lucene.Net.QueryParsers.ParseException expected)
			{
				// too many boolean clauses, so ParseException is expected
			}

            BooleanQuery.SetMaxClauseCount(originalMaxClauses);
		}
		
        [TestFixtureTearDown]
		public virtual void  TearDown()
		{
			BooleanQuery.SetMaxClauseCount(originalMaxClauses);
		}
	}
}