#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System.Globalization;
using DbLinq;
using DbLinq.Schema;
using DbLinq.Schema.Implementation;
using DbLinq.Util;
using DbMetal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using Case = DbLinq.Schema.Case;
using WordsExtraction = DbLinq.Schema.WordsExtraction;

namespace DbLinqTest
{
    /// <summary>
    /// Test for NameFormatter
    /// </summary>
    [TestFixture]
    [TestClass]
    public class NameFormatterTest
    {
        private NameFormat InvariantNameFormat
        {
            get
            {
                return new NameFormat(false, Case.PascalCase, CultureInfo.InvariantCulture);
            }
        }

        private NameFormat EnglishNameFormat
        {
            get
            {
                Reference.DbLinqLocalizations();
                return new NameFormat(false, Case.PascalCase, new CultureInfo("en-us"));
            }
        }

        [TestMethod]
        [Test]
        public void InvalidCharactersCaseTest()
        {
            var nf = new NameFormatter();
            var tn = nf.GetTableName("A#?", WordsExtraction.FromCase, InvariantNameFormat);
            Assert.AreEqual("A__", tn.ClassName);
        }

        [TestMethod]
        [Test]
        public void InvalidCharactersLanguageTest()
        {
            var nf = new NameFormatter();
            var tn = nf.GetTableName("A#?", WordsExtraction.FromDictionary, InvariantNameFormat);
            Assert.AreEqual("A__", tn.ClassName);
        }

        [TestMethod]
        [Test]
        public void InvalidCharactersLanguage2Test()
        {
            var nf = new NameFormatter();
            var tn = nf.GetTableName("Test#?", WordsExtraction.FromDictionary, EnglishNameFormat);
            Assert.AreEqual("Test__", tn.ClassName);
        }

        [TestMethod]
        [Test]
        public void GetWordsTest_MyTableName()
        {
            var nf = new NameFormatter();
            var tn = nf.GetTableName("MY_TABLE_NAME_", WordsExtraction.FromDictionary, EnglishNameFormat);
            Assert.AreEqual("MyTableName", tn.ClassName);
        }

        [TestMethod]
        [Test]
        public void GetWordsTest_MyTableName2()
        {
            var nf = new NameFormatter();
            var tn = nf.GetTableName("_MY_TABLE__NAME", WordsExtraction.FromDictionary, EnglishNameFormat);
            Assert.AreEqual("MyTableName", tn.ClassName);
        }

    }
}
