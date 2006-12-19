//
// TaskItemTest.cs:
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
using System.Collections;
using System.Collections.Specialized;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Utilities {

	[TestFixture]
	public class TaskItemTest {

		ITaskItem item,item1,item2;
		ICollection metadataNames;

		[SetUp]
		public void SetUp ()
		{
			string[] temp = new string[] {"FullPath", "RootDir", "Filename", "Extension", "RelativeDir", "Directory",
				"RecursiveDir", "Identity", "ModifiedTime", "CreatedTime", "AccessedTime"};
			ArrayList al = new ArrayList ();
			foreach (string s in temp)
				al.Add (s);
			metadataNames = al;
		}
		
		private bool CompareStringCollections (ICollection compared, ICollection reference)
		{
			Hashtable comparedHash;
			comparedHash = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			
			foreach (string s in compared)
				comparedHash.Add (s, null);
			
			foreach (string s in reference) {
				if (comparedHash.ContainsKey (s) == false) {
					return false;
				}
			}
			
			return true;
		}

		public void TestCloneCustomMetadata ()
		{
			item = new TaskItem ();
			item.SetMetadata ("AAA", "111");
			item.SetMetadata ("aaa", "222");
			item.SetMetadata ("BBB", "111");

			string [] metakeys = new string [] { "aaa", "BBB" };
			IDictionary meta = item.CloneCustomMetadata ();

			Assert.IsTrue (CompareStringCollections (meta.Keys, metakeys), "A1");
			metakeys [0] = "aAa";
			Assert.IsTrue (CompareStringCollections (meta.Keys, metakeys), "A2");
			Assert.AreEqual ("222", meta ["aaa"], "A3");
			Assert.AreEqual ("222", meta ["AAA"], "A4");
			Assert.AreEqual ("222", meta ["aAa"], "A5");
			Assert.AreEqual ("111", meta ["BbB"], "A5");
		}

		[Test]
		[Ignore ("NRE on .NET 2.0")]
		public void TestCtor1 ()
		{
			new TaskItem ((ITaskItem) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtor2 ()
		{
			new TaskItem ((string) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtor3 ()
		{
			new TaskItem ((string) null, new Hashtable ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtor4 ()
		{
			new TaskItem ("itemspec", null);
		}

		[Test]
		public void TestCopyConstructor ()
		{
			item1 = new TaskItem ("itemSpec");
			item1.SetMetadata ("meta1", "val1");
			item2 = new TaskItem (item1);
			Assert.AreEqual (item1.GetMetadata ("meta1"), item2.GetMetadata ("meta1"), "A1");
			item1.SetMetadata ("meta1", "val2");
			Assert.AreEqual ("val2", item1.GetMetadata ("meta1"), "A2");
			Assert.AreEqual ("val1", item2.GetMetadata ("meta1"), "A3");
			item2.SetMetadata ("meta1", "val3");
			Assert.AreEqual ("val2", item1.GetMetadata ("meta1"), "A4");
			Assert.AreEqual ("val3", item2.GetMetadata ("meta1"), "A5");
		}

		[Test]
		public void TestCopyMetadataTo ()
		{
			item1 = new TaskItem ("itemSpec");
			item2 = new TaskItem ("itemSpec");
			item1.SetMetadata ("A", "1");
			item1.SetMetadata ("B", "1");
			item1.SetMetadata ("C", "1");
			item2.SetMetadata ("B", "2");
			item1.CopyMetadataTo (item2);
			Assert.AreEqual ("1", item2.GetMetadata ("A"), "1");
			Assert.AreEqual ("2", item2.GetMetadata ("B"), "2");
			Assert.AreEqual ("1", item2.GetMetadata ("C"), "3");
		}

		[Test]
		public void TestGetMetadata ()
		{
			item = new TaskItem ("itemSpec");
			item.SetMetadata ("Metadata", "Value");
			Assert.AreEqual ("Value", item.GetMetadata ("Metadata"), "A1");
			Assert.AreEqual (String.Empty, item.GetMetadata ("lala"), "A2");
			Assert.AreEqual ("itemSpec", item.GetMetadata ("iDentity"), "A3");
			Assert.AreEqual ("", item.GetMetadata ("extension"), "A4");
			Assert.AreEqual ("", item.GetMetadata ("ModifiedTime"), "A5");
			Assert.AreEqual ("", item.GetMetadata ("CreatedTime"), "A6");
			Assert.AreEqual ("", item.GetMetadata ("ModifiedTime"), "A7");
			Assert.AreEqual ("", item.GetMetadata ("AccessedTime"), "A8");
		}

		[Test]
		public void TestMetadataNames ()
		{
			item = new TaskItem ("itemSpec");

			Assert.IsTrue (CompareStringCollections (item.MetadataNames, metadataNames), "A1");

			item.SetMetadata ("a", "b");

			Assert.AreEqual (12, item.MetadataNames.Count, "A2");
		}

		[Test]
		public void TestOpExplicit ()
		{
			TaskItem item = new TaskItem ("itemSpec");
			item.SetMetadata ("a", "b");

			Assert.AreEqual ("itemSpec", (string) item, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestRemoveMetadata1 ()
		{
			item = new TaskItem ("lalala");
			item.RemoveMetadata ("EXTension");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveMetadata2 ()
		{
			item = new TaskItem ("lalala");
			item.RemoveMetadata (null);
		}

		[Test]
		public void TestRemoveMetadata3 ()
		{
			item = new TaskItem ("lalala");
			item.SetMetadata ("a", "b");
			item.RemoveMetadata ("a");

			Assert.AreEqual (11, item.MetadataCount, "A1");
		}

		[Test]
		public void TestSetMetadata1 ()
		{
			item = new TaskItem ("itemSpec");
			item.SetMetadata ("Metadata", "Value1");
			item.SetMetadata ("Metadata", "Value2");
			Assert.AreEqual (item.MetadataCount, 12, "MetadataCount");
			Assert.AreEqual ("Value2", item.GetMetadata ("Metadata"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestSetMetadata2 ()
		{
			item = new TaskItem ("itemSpec");
			item.SetMetadata (null, "value");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestSetMetadata3 ()
		{
			item = new TaskItem ("itemSpec");
			item.SetMetadata ("name", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestSetReservedMetadata ()
		{
			item = new TaskItem ("lalala");
			item.SetMetadata ("Identity", "some value");
		}
	}
}
