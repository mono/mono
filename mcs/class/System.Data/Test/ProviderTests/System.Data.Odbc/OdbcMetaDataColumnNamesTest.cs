// OdbcMetaDataColumnNamesTest.cs - NUnit Test Cases for testing the
//                          OdbcMetaDataColumnNames class
// Author:
//      Nidhi Rawal (rawalnidhi_rawal@yahoo.com)
//
// Copyright (c) 2007 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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
//

using System;
using System.Data;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
{

	[TestFixture]
	[Category ("odbc")]
	public class OdbcMetaDataColumnNamesTest
	{
		[Test]
		public void ReferenceEqualsTest ()
		{
			OdbcMetaDataColumnNames om = new OdbcMetaDataColumnNames ();
			object o1 = null;
			object o2 = null;
			object o3 = new object ();

			Assert.AreEqual(true, OdbcOdbcMetaDataColumnNames.ReferenceEquals (o1, o2),"#1 Comparing the references of two objects");
			o2 = o3;
			Assert.AreEqual(true, OdbcOdbcMetaDataColumnNames.ReferenceEquals (o2, o3), "#2 Comparing the references of two objects after making them equal");
			Assert.AreEqual(false, OdbcOdbcMetaDataColumnNames.ReferenceEquals (o1, o2), "#3 Comparing the references of two objects that we compared earlier");
		}
	}
}
