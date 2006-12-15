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
	
			Assert.AreEqual (itemInclude, item.FinalItemSpec, "A1");
			Assert.AreEqual (itemInclude, item.Include, "A2");
			Assert.AreEqual (String.Empty, item.Exclude, "A3");
			Assert.AreEqual (String.Empty, item.Condition, "A4");
			Assert.AreEqual (false, item.IsImported, "A5");
			Assert.AreEqual (itemName, item.Name, "A6");
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
	
			Assert.AreEqual (itemSpec, item.FinalItemSpec, "A1");
			Assert.AreEqual (escapedInclude, item.Include, "A2");
			Assert.AreEqual (String.Empty, item.Exclude, "A3");
			Assert.AreEqual (String.Empty, item.Condition, "A4");
			Assert.AreEqual (false, item.IsImported, "A5");
			Assert.AreEqual (itemName, item.Name, "A6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtor3 ()
		{
			new BuildItem (null, (string) null);
		}

		[Test]
		[Ignore ("NullRefException on MS .NET 2.0")]
		public void TestCtor4 ()
		{
			new BuildItem (null, (ITaskItem) null);
		}

		[Test]
		public void TestCtor5 ()
		{
			new BuildItem (null, "something");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException),
			"Parameter \"itemInclude\" cannot have zero length.")]
		public void TestCtor6 ()
		{
			new BuildItem (null, String.Empty);
		}

		[Test]
		[Ignore ("IndexOutOfRange on MS .NET 2.0")]
		public void TestCtor7 ()
		{
			new BuildItem (String.Empty, "something");
		}

		[Test]
		public void TestClone1 ()
		{
			item = new BuildItem ("name", "1;2;3");
			item.SetMetadata ("a", "b");

			BuildItem item2 = item.Clone ();

			Assert.AreEqual ("1;2;3", item2.FinalItemSpec, "A1");
			Assert.AreEqual ("1;2;3", item2.Include, "A2");
			Assert.AreEqual (String.Empty, item2.Exclude, "A3");
			Assert.AreEqual (String.Empty, item2.Condition, "A4");
			Assert.AreEqual (false, item2.IsImported, "A5");
			Assert.AreEqual ("name", item2.Name, "A6");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException),
			"Cannot set a condition on an object not represented by an XML element in the project file.")]
		public void TestCondition ()
		{
			item = new BuildItem ("name", "1");
			item.Condition = "true";
		}

		[Test]
		public void TestCopyCustomMetadataTo1 ()
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

			Assert.AreEqual (metadataValue, destination.GetMetadata (metadataName), "A1");
			Assert.AreEqual (metadataValue, destination.GetEvaluatedMetadata (metadataName), "A2");
		}

		// NOTE: it's weird that they don't throw ArgumentNullException
		[Test]
		[Ignore ("MS throws NullRefException")]
		public void TestCopyCustomMetadataTo2 ()
		{
			BuildItem item = new BuildItem ("name", "include");
			item.SetMetadata ("name", "value");
			
			item.CopyCustomMetadataTo (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException),
			"Assigning the \"Exclude\" attribute of a virtual item is not allowed.")]
		public void TestExclude ()
		{
			item = new BuildItem ("name", "1");
			item.Exclude = "e";
		}

		[Test]
		public void TestHasMetadata ()
		{
			string itemName = "a";
			string itemInclude = "a";
			string metadataName = "name";

			item = new BuildItem (itemName, itemInclude);

			Assert.AreEqual (false, item.HasMetadata (metadataName), "A1");
            
			item.SetMetadata (metadataName, "value");

			Assert.AreEqual (true, item.HasMetadata (metadataName), "A2");
		}

		[Test]
		public void TestGetMetadata ()
		{
			string itemName = "a";
			string itemInclude = "a";
			string metadataName = "name";
			string metadataValue = "a;b;c";

			item = new BuildItem (itemName, itemInclude);

			Assert.AreEqual (String.Empty, item.GetMetadata (metadataName), "A1");

			item.SetMetadata (metadataName, metadataValue);

			Assert.AreEqual (metadataValue, item.GetMetadata (metadataName), "A2");
		}

		[Test]
		public void TestGetEvaluatedMetadata ()
		{
			string itemName = "a";
			string itemInclude = "a";
			string metadataName = "name";
			string metadataValue = "a;b;c";

			item = new BuildItem (itemName, itemInclude);

			Assert.AreEqual (String.Empty, item.GetEvaluatedMetadata (metadataName), "A1");

			item.SetMetadata (metadataName, metadataValue);

			Assert.AreEqual (metadataValue, item.GetEvaluatedMetadata (metadataName), "A2");
		}

		[Test]
		public void TestRemoveMetadata1 ()
		{
			string itemName = "a";
			string itemInclude = "a";
			string metadataName = "name";
			string metadataValue = "a;b;c";

			item = new BuildItem (itemName, itemInclude);

			item.SetMetadata (metadataName, metadataValue);

			Assert.AreEqual (true, item.HasMetadata (metadataName), "A1");

			item.RemoveMetadata (metadataName);

			Assert.AreEqual (false, item.HasMetadata (metadataName), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveMetadata2 ()
		{
			item = new BuildItem ("name", "value");
			item.RemoveMetadata (null);
		}

		[Test]
		public void TestRemoveMetadata3 ()
		{
			item = new BuildItem ("name", "value");
			item.RemoveMetadata ("undefined_metadata");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestSetMetadata1 ()
		{
			item = new BuildItem ("name", "include");
			item.SetMetadata (null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestSetMetadata2 ()
		{
			item = new BuildItem ("name", "include");
			item.SetMetadata ("name", null);
		}

		[Test]
		public void TestSetMetadata3 ()
		{
			item = new BuildItem ("name", "include");
			item.SetMetadata ("a", "$(A)");
			item.SetMetadata ("b", "$(A)", true);
			item.SetMetadata ("c", "$(A)", false);

			Assert.AreEqual ("$(A)", item.GetEvaluatedMetadata ("a"), "A1");
			Assert.AreEqual ("$(A)", item.GetEvaluatedMetadata ("b"), "A2");
			Assert.AreEqual ("$(A)", item.GetEvaluatedMetadata ("c"), "A3");
			Assert.AreEqual ("$(A)", item.GetMetadata ("a"), "A4");
			Assert.AreEqual (Utilities.Escape ("$(A)"), item.GetMetadata ("b"), "A5");
			Assert.AreEqual ("$(A)", item.GetMetadata ("c"), "A6");
		}
	}
}
