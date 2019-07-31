//
// BinaryFormatterTest.cs - Unit tests for 
//	System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.Globalization {

	[TestFixture]
	public class DaylightTimeTest {

		[Test]
		public void Constructor ()
		{
			DaylightTime dt = new DaylightTime (DateTime.MinValue, DateTime.MaxValue, TimeSpan.MinValue);
			Assert.AreEqual (DateTime.MinValue, dt.Start, "Start");
			Assert.AreEqual (DateTime.MaxValue, dt.End, "End");
			Assert.AreEqual (TimeSpan.MinValue, dt.Delta, "Delta");
		}

		[Test]
		public void SerializationRoundtrip ()
		{
			DaylightTime dt = new DaylightTime (DateTime.MinValue, DateTime.MaxValue, TimeSpan.MinValue);
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, dt);

			ms.Position = 0;
			DaylightTime clone = (DaylightTime) bf.Deserialize (ms);

			Assert.AreEqual (clone.Start, dt.Start, "Start");
			Assert.AreEqual (clone.End, dt.End, "End");
			Assert.AreEqual (clone.Delta, dt.Delta, "Delta");
		}

		static private byte[] serialized_daylighttime = {
			0x0, 0x1, 0x0, 0x0, 0x0, 0xFF, 0xFF, 0xFF, 0xFF, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x1, 0x0, 0x0, 
			0x0, 0x21, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x47, 0x6C, 0x6F, 0x62, 0x61, 0x6C, 0x69, 0x7A, 0x61, 
			0x74, 0x69, 0x6F, 0x6E, 0x2E, 0x44, 0x61, 0x79, 0x6C, 0x69, 0x67, 0x68, 0x74, 0x54, 0x69, 0x6D, 0x65, 0x3, 
			0x0, 0x0, 0x0, 0x6, 0x5F, 0x73, 0x74, 0x61, 0x72, 0x74, 0x4, 0x5F, 0x65, 0x6E, 0x64, 0x6, 0x5F, 0x64, 0x65, 
			0x6C, 0x74, 0x61, 0x0, 0x0, 0x0, 0xD, 0xD, 0xC, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xFF, 0x3F, 0x37, 
			0xF4, 0x75, 0x28, 0xCA, 0x2B, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x80, 0xB
		};

		[Test]
		public void DeserializeKnownValue ()
		{
			MemoryStream ms = new MemoryStream (serialized_daylighttime);
			BinaryFormatter bf = new BinaryFormatter ();
			DaylightTime dt = (DaylightTime) bf.Deserialize (ms);
			Assert.AreEqual (DateTime.MinValue, dt.Start, "Start");
			Assert.AreEqual (DateTime.MaxValue, dt.End, "End");
			Assert.AreEqual (TimeSpan.MinValue, dt.Delta, "Delta");
		}
	}
}
