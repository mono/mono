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

using DbMetal.Language;
using System.Collections.Generic;
using NUnit.Framework;

namespace DbLinqTest
{


    /// <summary>
    ///This is a test class for EnglishWordsTest and is intended
    ///to contain all EnglishWordsTest Unit Tests
    ///</summary>
    [TestFixture]
    public class FrenchWordsTest
    {
        public FrenchWordsTest()
        {
            frenchWords = new FrenchWords();
            frenchWords.Load();
        }

        public static void AssertAreIListEqual(IList<string> a, IList<string> b)
        {
            Assert.AreEqual(b.Count, a.Count);
            for (int index = 0; index < a.Count; index++)
                Assert.AreEqual(b[index], a[index]);
        }

        public static void AssertAreEqual(IList<string> a, params string[] b)
        {
            AssertAreIListEqual(a, b);
        }

        /*
        hiredate
        quantityperunit
        unitsinstock
        fkterrregion
        fkprodcatg
        */

        private FrenchWords frenchWords;

        [Test]
        public void GetWordsTest_SalutMonde()
        {
            var actual = frenchWords.GetWords("salutmonde");
            AssertAreEqual(actual, "salut", "monde");
        }

        [Test]
        public void GetWordsTest_MTER()
        {
            var actual = frenchWords.GetWords("montailleurestriche");
            AssertAreEqual(actual, "mon", "tailleur", "est", "riche");
        }

        [Test]
        public void PluralizeTest_Oeuf()
        {
            var actual = frenchWords.Pluralize("œuf");
            Assert.AreEqual("œufs", actual);
        }

        [Test]
        public void PluralizeTest_Bijou()
        {
            var actual = frenchWords.Pluralize("bijou");
            Assert.AreEqual("bijoux", actual);
        }

        [Test]
        public void PluralizeTest_Cou()
        {
            var actual = frenchWords.Pluralize("cou");
            Assert.AreEqual("cous", actual);
        }

        [Test]
        public void PluralizeTest_Gas()
        {
            var actual = frenchWords.Pluralize("gas");
            Assert.AreEqual("gas", actual);
        }
    }
}
