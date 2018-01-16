//
// EndpointAddressBuilderTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class EndpointAddressBuilderTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToEndpointAddressWithoutReader ()
		{
			new EndpointAddressBuilder ().ToEndpointAddress ();
		}

		[Test]
		public void UsageExample ()
		{
			var eb = new EndpointAddressBuilder ();
			var dr = XmlDictionaryReader.CreateDictionaryReader (XmlReader.Create (new StringReader ("<foo/>")));
			eb.SetExtensionReader (dr);
			Assert.AreEqual (ReadState.EndOfFile, dr.ReadState, "#1");
			var xr = eb.GetReaderAtExtensions ();
			xr.ReadOuterXml ();
			xr = eb.GetReaderAtExtensions (); // do not return the same XmlReader
			Assert.AreEqual (ReadState.Interactive, xr.ReadState, "#2");
			xr.ReadOuterXml ();
			eb.SetExtensionReader (null); // allowed
			Assert.IsNull (eb.GetReaderAtExtensions (), "#3");
		}
	}
}
#endif
