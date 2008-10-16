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
namespace Lucene.Net.Analysis.RU
{
	[TestFixture]
	public class TestRussianStem
	{
		private System.Collections.ArrayList words = new System.Collections.ArrayList();
		private System.Collections.ArrayList stems = new System.Collections.ArrayList();
		
		/// <seealso cref="TestCase#setUp()">
		/// </seealso>
		[TestFixtureSetUp]
		protected virtual void  SetUp()
		{
			//base.SetUp();
			//System.out.println(new java.util.Date());
			System.String str;
			System.IO.FileInfo dataDir = new System.IO.FileInfo(SupportClass.AppSettings.Get("dataDir", @".\"));
			
			// open and read words into an array list
			System.IO.StreamReader inWords = new System.IO.StreamReader(
                new System.IO.StreamReader(
                    new System.IO.FileStream(
                        new System.IO.FileInfo(
                            dataDir.FullName + @"\Analysis\RU\wordsUnicode.txt").FullName, 
                            System.IO.FileMode.Open, System.IO.FileAccess.Read), 
                            System.Text.Encoding.GetEncoding("Unicode")).BaseStream, 
                            new System.IO.StreamReader(
                                new System.IO.FileStream(
                                    new System.IO.FileInfo(
                                        dataDir.FullName + @"\Analysis\RU\wordsUnicode.txt").FullName, 
                                        System.IO.FileMode.Open, 
                                        System.IO.FileAccess.Read), 
                                        System.Text.Encoding.GetEncoding("Unicode")).CurrentEncoding);
			while ((str = inWords.ReadLine()) != null)
			{
				words.Add(str);
			}
			inWords.Close();
			
			// open and read stems into an array list
			System.IO.StreamReader inStems = new System.IO.StreamReader(
                new System.IO.StreamReader(
                    new System.IO.FileStream(
                        new System.IO.FileInfo(
                            dataDir.FullName + @"\Analysis\RU\stemsUnicode.txt").FullName, 
                            System.IO.FileMode.Open, 
                            System.IO.FileAccess.Read), 
                            System.Text.Encoding.GetEncoding("Unicode")).BaseStream, 
                            new System.IO.StreamReader(
                                new System.IO.FileStream(
                                    new System.IO.FileInfo(
                                        dataDir.FullName + @"\Analysis\RU\stemsUnicode.txt").FullName, 
                                        System.IO.FileMode.Open, 
                                        System.IO.FileAccess.Read), 
                                        System.Text.Encoding.GetEncoding("Unicode")).CurrentEncoding);
			while ((str = inStems.ReadLine()) != null)
			{
				stems.Add(str);
			}
			inStems.Close();
		}
		
		/// <seealso cref="TestCase#tearDown()">
		/// </seealso>
		[TestFixtureTearDown]
		protected virtual void  TearDown()
		{
			//base.TearDown();
		}
		
        [Test]
		public virtual void  TestStem()
		{
			for (int i = 0; i < words.Count; i++)
			{
				//if ( (i % 100) == 0 ) System.err.println(i);
				System.String realStem = RussianStemmer.Stem((System.String) words[i], RussianCharsets.UnicodeRussian);
				Assert.AreEqual(stems[i], realStem, "unicode");
			}
		}
		
		private System.String printChars(System.String output)
		{
			System.Text.StringBuilder s = new System.Text.StringBuilder();
			for (int i = 0; i < output.Length; i++)
			{
				s.Append(output[i]);
			}
			return s.ToString();
		}
	}
}