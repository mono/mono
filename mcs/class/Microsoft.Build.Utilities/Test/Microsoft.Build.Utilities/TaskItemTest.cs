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
			comparedHash = new Hashtable ();
			
			foreach (string s in compared)
				comparedHash.Add (s, null);
			
			foreach (string s in reference) {
				if (comparedHash.ContainsKey (s) == false) {
					Console.Error.WriteLine ("{0} not found", s);
					return false;
				}
			}
			
			return true;
		}
		
		[Test]
		public void TestSetMetadata ()
		{
			item = new TaskItem ("itemSpec");
			item.SetMetadata ("Metadata", "Value");
			Assert.AreEqual (item.MetadataCount, 12, "MetadataCount");
		}
		
		[Test]
		public void TestGetMetadata ()
		{
			item = new TaskItem ("itemSpec");
			item.SetMetadata ("Metadata", "Value");
			Assert.AreEqual (item.GetMetadata ("Metadata"), "Value", "Metadata value");
		}
		
		[Test]
		public void TestCopyMetadataTo ()
		{
			item1 = new TaskItem ("itemSpec");
			item2 = new TaskItem ("itemSpec");
			item1.SetMetadata ("1","2");
			item1.CopyMetadataTo (item2);
			Assert.AreEqual (item2.GetMetadata ("1"), item1.GetMetadata ("1"),"Metadata in items");
		}
		
		[Test]
		public void TestMetadataNames ()
		{
			item = new TaskItem ("itemSpec");

			Assert.IsTrue (CompareStringCollections (item.MetadataNames, metadataNames));
		}	
	}
}