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
using Token = Lucene.Net.Analysis.Token;
using TokenStream = Lucene.Net.Analysis.TokenStream;
namespace Lucene.Net.Analysis.RU
{
	/// <summary> Test case for RussianAnalyzer.
	/// 
	/// </summary>
	/// <author>     Boris Okner
	/// </author>
	/// <version>    $Id: TestRussianAnalyzer.java,v 1.6 2004/03/29 22:48:06 cutting Exp $
	/// </version>
	
    [TestFixture]
	public class TestRussianAnalyzer
	{
		private System.IO.StreamReader inWords;
		
		private System.IO.StreamReader sampleUnicode;
		
		private System.IO.StreamReader inWordsKOI8;
		
		private System.IO.StreamReader sampleKOI8;
		
		private System.IO.StreamReader inWords1251;
		
		private System.IO.StreamReader sample1251;
		
		private System.IO.FileInfo dataDir;
		
        [TestFixtureSetUp]
		protected virtual void  SetUp()
		{
			dataDir = new System.IO.FileInfo(SupportClass.AppSettings.Get("dataDir", @".\"));
		}
		
        [Test]
		public virtual void  TestUnicode()
		{
			RussianAnalyzer ra = new RussianAnalyzer(RussianCharsets.UnicodeRussian);
			inWords = new System.IO.StreamReader(
                new System.IO.FileStream(
                    new System.IO.FileInfo(
                        dataDir.FullName + @"Analysis\RU\testUnicode.txt").FullName, 
                        System.IO.FileMode.Open, 
                        System.IO.FileAccess.Read), 
                    System.Text.Encoding.GetEncoding("Unicode"));
			
			sampleUnicode = new System.IO.StreamReader(
                new System.IO.FileStream(
                    new System.IO.FileInfo(
                        dataDir.FullName + @"Analysis\RU\resUnicode.htm").FullName, 
                        System.IO.FileMode.Open,
                        System.IO.FileAccess.Read), 
                    System.Text.Encoding.GetEncoding("Unicode"));
			
			TokenStream in_Renamed = ra.TokenStream("all", inWords);
			
			RussianLetterTokenizer sample = new RussianLetterTokenizer(sampleUnicode, RussianCharsets.UnicodeRussian);
			
			for (; ; )
			{
				Token token = in_Renamed.Next();
				
				if (token == null)
				{
					break;
				}
				
				Token sampleToken = sample.Next();
				Assert.AreEqual(token.TermText(), sampleToken == null ? null : sampleToken.TermText(), "Unicode");
			}
			
			inWords.Close();
			sampleUnicode.Close();
		}
		
        [Test]
		public virtual void  TestKOI8()
		{
			//System.out.println(new java.util.Date());
			RussianAnalyzer ra = new RussianAnalyzer(RussianCharsets.KOI8);
			// KOI8
			inWordsKOI8 = new System.IO.StreamReader(
                new System.IO.FileStream(
                    new System.IO.FileInfo(
                        dataDir.FullName + @"Analysis\RU\testKOI8.txt").FullName, 
                        System.IO.FileMode.Open, 
                        System.IO.FileAccess.Read), 
                        System.Text.Encoding.GetEncoding("iso-8859-1"));
			
			sampleKOI8 = new System.IO.StreamReader(
                new System.IO.FileStream(
                    new System.IO.FileInfo(dataDir.FullName + @"Analysis\RU\resKOI8.htm").FullName, 
                        System.IO.FileMode.Open, 
                        System.IO.FileAccess.Read), 
                        System.Text.Encoding.GetEncoding("iso-8859-1"));
			
			TokenStream in_Renamed = ra.TokenStream("all", inWordsKOI8);
			RussianLetterTokenizer sample = new RussianLetterTokenizer(sampleKOI8, RussianCharsets.KOI8);
			
			for (; ; )
			{
				Token token = in_Renamed.Next();
				
				if (token == null)
				{
					break;
				}
				
				Token sampleToken = sample.Next();
				Assert.AreEqual(token.TermText(), sampleToken == null ? null : sampleToken.TermText(), "KOI8");
			}
			
			inWordsKOI8.Close();
			sampleKOI8.Close();
		}
		
        [Test]
		public virtual void  Test1251()
		{
			// 1251
			inWords1251 = new System.IO.StreamReader(
                new System.IO.FileStream(
                    new System.IO.FileInfo(
                        dataDir.FullName + @"Analysis\RU\test1251.txt").FullName, 
                        System.IO.FileMode.Open, 
                        System.IO.FileAccess.Read), 
                        System.Text.Encoding.GetEncoding("iso-8859-1"));
			
			sample1251 = new System.IO.StreamReader(
                new System.IO.FileStream(
                    new System.IO.FileInfo(
                        dataDir.FullName + @"Analysis\RU\res1251.htm").FullName, 
                        System.IO.FileMode.Open, 
                        System.IO.FileAccess.Read), 
                        System.Text.Encoding.GetEncoding("iso-8859-1"));
			
			RussianAnalyzer ra = new RussianAnalyzer(RussianCharsets.CP1251);
			TokenStream in_Renamed = ra.TokenStream("", inWords1251);
			RussianLetterTokenizer sample = new RussianLetterTokenizer(sample1251, RussianCharsets.CP1251);
			
			for (; ; )
			{
				Token token = in_Renamed.Next();
				
				if (token == null)
				{
					break;
				}
				
				Token sampleToken = sample.Next();
				Assert.AreEqual(token.TermText(), sampleToken == null ? null : sampleToken.TermText(), "1251");
			}
			
			inWords1251.Close();
			sample1251.Close();
		}
	}
}