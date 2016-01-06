// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
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

using NUnit.Framework.Api;

namespace NUnit.Framework.Internal
{
    [TestFixture]
    public class PropertyBagTests
    {
        PropertyBag bag;

        [SetUp]
        public void SetUp()
        {
            bag = new PropertyBag();
            bag.Add("Answer", 42);
            bag.Add("Tag", "bug");
            bag.Add("Tag", "easy");
        }

        [Test]
        public void CountReflectsNumberOfPairs()
        {
            Assert.That(bag.Count, Is.EqualTo(3));
        }

        [Test]
        public void IndexGetsListOfValues()
        {
            Assert.That(bag["Answer"].Count, Is.EqualTo(1));
            Assert.That(bag["Answer"], Contains.Item(42));

            Assert.That(bag["Tag"].Count, Is.EqualTo(2));
            Assert.That(bag["Tag"], Contains.Item("bug"));
            Assert.That(bag["Tag"], Contains.Item("easy"));
        }

        [Test]
        public void IndexGetsEmptyListIfNameIsNotPresent()
        {
            Assert.That(bag["Level"].Count, Is.EqualTo(0));
        }

        [Test]
        public void IndexSetsListOfValues()
        {
            bag["Zip"] = new string[] {"junk", "more junk"};
            Assert.That(bag["Zip"].Count, Is.EqualTo(2));
            Assert.That(bag["Zip"], Contains.Item("junk"));
            Assert.That(bag["Zip"], Contains.Item("more junk"));
        }

        [Test]
        public void CanClearTheBag()
        {
            bag.Clear();
            Assert.That(bag.Keys.Count, Is.EqualTo(0));
            Assert.That(bag.Count, Is.EqualTo(0));
        }

        [Test]
        public void AllKeysAreListed()
        {
            Assert.That(bag.Keys.Count, Is.EqualTo(2));
            Assert.That(bag.Keys, Has.Member("Answer"));
            Assert.That(bag.Keys, Has.Member("Tag"));
        }

        [Test]
        public void ContainsKey()
        {
            Assert.That(bag.ContainsKey("Answer"));
            Assert.That(bag.ContainsKey("Tag"));
            Assert.False(bag.ContainsKey("Target"));
        }

        [Test]
        public void ContainsKeyAndValue()
        {
            Assert.That(bag.Contains("Answer", 42));
        }

        [Test]
        public void ContainsPropertyEntry()
        {
            Assert.That(bag.Contains(new PropertyEntry("Answer", 42)));
        }

        [Test]
        public void CanRemoveKey()
        {
            bag.Remove("Tag");
            Assert.That(bag.Keys.Count, Is.EqualTo(1));
            Assert.That(bag.Count, Is.EqualTo(1));
            Assert.That(bag.Keys, Has.No.Member("Tag"));
        }

        [Test]
        public void CanRemoveMissingKeyWithoutError()
        {
            bag.Remove("Zip");
        }

        [Test]
        public void CanRemoveNameAndValue()
        {
            bag.Remove("Tag", "easy");
            Assert.That(bag["Tag"].Contains("bug"));
            Assert.False(bag["Tag"].Contains("easy"));
            Assert.That(bag.Count, Is.EqualTo(2));
        }

        [Test]
        public void CanRemoveNameAndMissingValueWithoutError()
        {
            bag.Remove("Tag", "wishlist");
        }

        [Test]
        public void CanRemovePropertyEntry()
        {
            bag.Remove(new PropertyEntry("Tag", "easy"));
            Assert.That(bag["Tag"].Contains("bug"));
            Assert.False(bag["Tag"].Contains("easy"));
            Assert.That(bag.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetReturnsSingleValue()
        {
            Assert.That(bag.Get("Answer"), Is.EqualTo(42));
            Assert.That(bag.Get("Tag"), Is.EqualTo("bug"));
        }

        [Test]
        public void SetAddsNewSingleValue()
        {
            bag.Set("Zip", "ZAP");
            Assert.That(bag["Zip"].Count, Is.EqualTo(1));
            Assert.That(bag["Zip"], Has.Member("ZAP"));
            Assert.That(bag.Get("Zip"), Is.EqualTo("ZAP"));
        }

        [Test]
        public void SetReplacesOldValues()
        {
            bag.Set("Tag", "ZAPPED");
            Assert.That(bag["Tag"].Count, Is.EqualTo(1));
            Assert.That(bag.Get("Tag"), Is.EqualTo("ZAPPED"));
        }

        [Test]
        public void EnumeratorReturnsAllEntries()
        {
            int count = 0;
            // NOTE: Ignore unsuppressed warning about entry in .NET 1.1 build
            foreach (PropertyEntry entry in bag)
                ++count;
            Assert.That(count, Is.EqualTo(3));

        }
    }
}
