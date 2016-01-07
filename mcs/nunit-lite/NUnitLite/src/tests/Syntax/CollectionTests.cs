// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections;
using NUnit.TestUtilities;

namespace NUnit.Framework.Syntax
{
    public class UniqueTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<uniqueitems>";
            staticSyntax = Is.Unique;
            inheritedSyntax = Helper().Unique;
            builderSyntax = Builder().Unique;
        }
    }

    public class CollectionOrderedTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<ordered>";
            staticSyntax = Is.Ordered;
            inheritedSyntax = Helper().Ordered;
            builderSyntax = Builder().Ordered;
        }
    }

    public class CollectionOrderedTest_Descending : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<ordered descending>";
            staticSyntax = Is.Ordered.Descending;
            inheritedSyntax = Helper().Ordered.Descending;
            builderSyntax = Builder().Ordered.Descending;
        }
    }

    public class CollectionOrderedTest_Comparer : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            IComparer comparer = new SimpleObjectComparer();
            parseTree = "<ordered NUnit.TestUtilities.SimpleObjectComparer>";
            staticSyntax = Is.Ordered.Using(comparer);
            inheritedSyntax = Helper().Ordered.Using(comparer);
            builderSyntax = Builder().Ordered.Using(comparer);
        }
    }

    public class CollectionOrderedTest_Comparer_Descending : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            IComparer comparer = new SimpleObjectComparer();
            parseTree = "<ordered descending NUnit.TestUtilities.SimpleObjectComparer>";
            staticSyntax = Is.Ordered.Using(comparer).Descending;
            inheritedSyntax = Helper().Ordered.Using(comparer).Descending;
            builderSyntax = Builder().Ordered.Using(comparer).Descending;
        }
    }

    public class CollectionOrderedByTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<orderedby SomePropertyName>";
            staticSyntax = Is.Ordered.By("SomePropertyName");
            inheritedSyntax = Helper().Ordered.By("SomePropertyName");
            builderSyntax = Builder().Ordered.By("SomePropertyName");
        }
    }

    public class CollectionOrderedByTest_Descending : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<orderedby SomePropertyName descending>";
            staticSyntax = Is.Ordered.By("SomePropertyName").Descending;
            inheritedSyntax = Helper().Ordered.By("SomePropertyName").Descending;
            builderSyntax = Builder().Ordered.By("SomePropertyName").Descending;
        }
    }

    public class CollectionOrderedByTest_Comparer : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<orderedby SomePropertyName NUnit.TestUtilities.SimpleObjectComparer>";
            staticSyntax = Is.Ordered.By("SomePropertyName").Using(new SimpleObjectComparer());
            inheritedSyntax = Helper().Ordered.By("SomePropertyName").Using(new SimpleObjectComparer());
            builderSyntax = Builder().Ordered.By("SomePropertyName").Using(new SimpleObjectComparer());
        }
    }

    public class CollectionOrderedByTest_Comparer_Descending : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<orderedby SomePropertyName descending NUnit.TestUtilities.SimpleObjectComparer>";
            staticSyntax = Is.Ordered.By("SomePropertyName").Using(new SimpleObjectComparer()).Descending;
            inheritedSyntax = Helper().Ordered.By("SomePropertyName").Using(new SimpleObjectComparer()).Descending;
            builderSyntax = Builder().Ordered.By("SomePropertyName").Using(new SimpleObjectComparer()).Descending;
        }
    }

    public class CollectionContainsTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<contains 42>";
            staticSyntax = Contains.Item(42);
            inheritedSyntax = Helper().Contains(42);
            builderSyntax = Builder().Contains(42);
        }
    }

    public class CollectionContainsTest_String : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<contains \"abc\">";
            staticSyntax = Contains.Item("abc");
            inheritedSyntax = Helper().Contains("abc");
            builderSyntax = Builder().Contains("abc");
        }
    }

#if !SILVERLIGHT
    public class CollectionContainsTest_Comparer : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<contains 42>";
            staticSyntax = Contains.Item(42).Using(Comparer.Default);
            inheritedSyntax = Helper().Contains(42).Using(Comparer.Default);
            builderSyntax = Builder().Contains(42).Using(Comparer.Default);
        }

        [Test]
        public void ComparerIsCalled()
        {
            TestComparer comparer = new TestComparer();
            Assert.That(new int[] { 1, 2, 3 },
                Contains.Item(2).Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        [Test]
        public void ComparerIsCalledInExpression()
        {
            TestComparer comparer = new TestComparer();
            Assert.That(new int[] { 1, 2, 3 },
                Has.Length.EqualTo(3).And.Contains(2).Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }
    }

    public class CollectionContainsTest_Comparer_String : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<contains \"abc\">";
            staticSyntax = Contains.Item("abc").Using(Comparer.Default);
            inheritedSyntax = Helper().Contains("abc").Using(Comparer.Default);
            builderSyntax = Builder().Contains("abc").Using(Comparer.Default);
        }

        [Test]
        public void ComparerIsCalled()
        {
            TestComparer comparer = new TestComparer();
            Assert.That(new string[] { "Hello", "World" },
                Contains.Item("World").Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        [Test]
        public void ComparerIsCalledInExpression()
        {
            TestComparer comparer = new TestComparer();
            Assert.That(new string[] { "Hello", "World" },
                Has.Length.EqualTo(2).And.Contains("World").Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }
    }

    public class CollectionMemberTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<contains 42>";
            staticSyntax = Has.Member(42);
            inheritedSyntax = Helper().Contains(42);
            builderSyntax = Builder().Contains(42);
        }
    }

    public class CollectionMemberTest_Comparer : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<contains 42>";
            staticSyntax = Has.Member(42).Using(Comparer.Default);
            inheritedSyntax = Helper().Contains(42).Using(Comparer.Default);
            builderSyntax = Builder().Contains(42).Using(Comparer.Default);
        }
    }
#endif

    public class CollectionSubsetTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            int[] ints = new int[] { 1, 2, 3 };
            parseTree = "<subsetof System.Int32[]>";
            staticSyntax = Is.SubsetOf(ints);
            inheritedSyntax = Helper().SubsetOf(ints);
            builderSyntax = Builder().SubsetOf(ints);
        }
    }

    public class CollectionEquivalentTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            int[] ints = new int[] { 1, 2, 3 };
            parseTree = "<equivalent System.Int32[]>";
            staticSyntax = Is.EquivalentTo(ints);
            inheritedSyntax = Helper().EquivalentTo(ints);
            builderSyntax = Builder().EquivalentTo(ints);
        }
    }
}
