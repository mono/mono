//
// System.ComponentModel.CollectionConverterTest.cs -
//	NUnit Test Cases for System.ComponentModel.CollectionConverter
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.ComponentModel;

namespace MonoTests.System.ComponentModel {

	[TestFixture]
	public class CollectionConverterTest {

		private CollectionConverter cc;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			cc = new CollectionConverter ();
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromString_Null ()
		{
			cc.ConvertFromString (null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromString_Empty ()
		{
			cc.ConvertFromString (String.Empty);
		}

		private const string array_of_strings  = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <string>go</string>\r\n  <string>mono</string>\r\n  </ArrayOfString>";

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromString ()
		{
			cc.ConvertFromString (array_of_strings);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom ()
		{
			cc.ConvertFrom (array_of_strings);
		}
	}
}
