//
// XmlMtomDictionaryWriterTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlMtomDictionaryWriterTest
	{
		// Nothing gave me either multi-parted MTOM message or
		// xop-included xml so far ...

		[Test]
		public void UseCase1 ()
		{
			MemoryStream ms = new MemoryStream ();
			var w = XmlDictionaryWriter.CreateMtomWriter (ms, Encoding.UTF8, 10000, "sTaRt", "myboundary", "urn:foo", false, false);
			w.WriteStartElement ("root");
			w.WriteRaw ("RAW");
			w.WriteStartElement ("foo");
			w.WriteChars (new char [] {'b', 'c', 'd'}, 0, 3);
			w.WriteBase64 (new byte [] {50, 60, 70}, 0, 3);
			w.WriteArray ("", "arr", "", new bool [] {true,false,true},0,3);
			w.WriteValue (new MyStreamProvider ());
			w.WriteString ("999\r\n\r\n666");
			w.WriteEndElement ();
			//w.WriteProcessingInstruction ("pi", "data");
			w.WriteEndElement (); // it outputs end of mime data.
			w.WriteStartElement ("root"); // it is in the next part).
			w.WriteEndElement (); // it does not close current part.
			w.WriteEndDocument (); // no effect.
			w.WriteStartElement ("root");
			w.WriteEndElement ();
			w.WriteStartElement ("root");
			w.WriteEndElement ();
			w.WriteEndDocument ();
			w.Flush ();
			ms.Position = 0;
			// there are some insiginificant output differences
			Assert.AreEqual (usecase1, new StreamReader (ms).ReadToEnd ().Replace ("<root />", "<root/>"));
		}

		string usecase1 = @"
--myboundary
Content-ID: <urn:foo>
Content-Transfer-Encoding: 8bit
Content-Type: application/xop+xml;charset=utf-8;type=""sTaRt""

<root>RAW<foo>bcdMjxG<arr>true</arr><arr>false</arr><arr>true</arr>AQIDBAU=999&#xD;XXX
&#xD;XXX
666</foo></root>
--myboundary--
<root/><root/><root/>".Replace ("\r\n", "\n").Replace ("\n", "\r\n").Replace ("XXX\r\n", "\n");
	}

	class MyStreamProvider : IStreamProvider
	{
		public Stream GetStream ()
		{
			return new MemoryStream (new byte [] {1, 2, 3, 4, 5});
		}

		public void ReleaseStream (Stream s)
		{
		}
	}
}

#endif