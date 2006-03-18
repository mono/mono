//
// BuildItemTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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

using System;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
    [TestFixture]
    public class BuildItemTest {

        BuildItem item;

        [Test]
        public void TestCtor1 ()
        {
            string itemName = "itemName";
            string itemInclude = "a;b;c";

            item = new BuildItem (itemName, itemInclude);

            Assert.AreEqual (item.FinalItemSpec, itemInclude, "A1");
            Assert.AreEqual (item.Include, itemInclude, "A2");
            Assert.AreEqual (item.Exclude, String.Empty, "A3");
            Assert.AreEqual (item.Condition, String.Empty, "A4");
            Assert.AreEqual (item.IsImported, false, "A5");
            Assert.AreEqual (item.Name, itemName, "A6");
        }

        [Test]
        public void TestCtor2 ()
        {
            string itemName = "itemName";
            string itemSpec = "a;b;c";
            // result of Utilities.Escape (itemSpec)
            string escapedInclude = "a%3bb%3bc";
            ITaskItem taskItem = new TaskItem (itemSpec);

            item = new BuildItem (itemName, taskItem);

            Assert.AreEqual (item.FinalItemSpec, itemSpec, "A1");
            Assert.AreEqual (item.Include, escapedInclude, "A2");
            Assert.AreEqual (item.Exclude, String.Empty, "A3");
            Assert.AreEqual (item.Condition, String.Empty, "A4");
            Assert.AreEqual (item.IsImported, false, "A5");
            Assert.AreEqual (item.Name, itemName, "A6");
        }

        [Test]
        public void TestCopyCustomMetadataTo ()
        {
            BuildItem source, destination;
            string itemName1 = "a";
            string itemName2 = "b";
            string itemInclude = "a;b;c";
            string metadataName = "name";
            string metadataValue = "value";

            source = new BuildItem (itemName1, itemInclude);
            destination = new BuildItem (itemName2, itemInclude);

            source.SetMetadata (metadataName, metadataValue);

            source.CopyCustomMetadataTo (destination);

            Assert.AreEqual (destination.GetMetadata (metadataName), metadataValue, "A1");
            Assert.AreEqual (destination.GetEvaluatedMetadata (metadataName), metadataValue, "A2");
        }

        [Test]
        public void TestHasMetadata ()
        {
            string itemName = "a";
            string itemInclude = "a";
            string metadataName = "name";

            item = new BuildItem (itemName, itemInclude);

            Assert.AreEqual (item.HasMetadata (metadataName), false, "A1");
            
            item.SetMetadata (metadataName, "value");

            Assert.AreEqual (item.HasMetadata (metadataName), true, "A2");
        }

        [Test]
        public void TestGetMetadata ()
        {
            string itemName = "a";
            string itemInclude = "a";
            string metadataName = "name";
            string metadataValue = "a;b;c";

            item = new BuildItem (itemName, itemInclude);

            Assert.AreEqual (item.GetMetadata (metadataName), String.Empty, "A1");

            item.SetMetadata (metadataName, metadataValue);

            Assert.AreEqual (item.GetMetadata (metadataName), metadataValue, "A2");
        }

        [Test]
        public void TestGetEvaluatedMetadata ()
        {
            string itemName = "a";
            string itemInclude = "a";
            string metadataName = "name";
            string metadataValue = "a;b;c";

            item = new BuildItem (itemName, itemInclude);

            Assert.AreEqual (item.GetEvaluatedMetadata (metadataName), String.Empty, "A1");

            item.SetMetadata (metadataName, metadataValue);

            Assert.AreEqual (item.GetEvaluatedMetadata (metadataName), metadataValue, "A2");
        }

        [Test]
        public void TestRemoveMetadata ()
        {
            string itemName = "a";
            string itemInclude = "a";
            string metadataName = "name";
            string metadataValue = "a;b;c";

            item = new BuildItem (itemName, itemInclude);

            item.SetMetadata (metadataName, metadataValue);

            Assert.AreEqual (item.HasMetadata (metadataName), true, "A1");

            item.RemoveMetadata (metadataName);

            Assert.AreEqual (item.HasMetadata (metadataName), false, "A2");
        }
    }
}