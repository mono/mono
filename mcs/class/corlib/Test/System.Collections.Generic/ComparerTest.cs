//
// MonoTests.System.Collections.Generic.Test.ComparerTest
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2007 Gert Driesen
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

#if NET_2_0

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.Collections.Generic
{
	[TestFixture]
	public class ComparerTest
	{

#if !NET_4_0 && !NET_2_1 // FIXME: the blob contains the 2.0 mscorlib version

		[Test] // bug #80929
		public void SerializeDefault ()
		{
			Comparer<int> c = Comparer<int>.Default;

			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, c);

			byte [] buffer = new byte [ms.Length];
			ms.Position = 0;
			ms.Read (buffer, 0, buffer.Length);

			Assert.AreEqual (_serializedDefault, buffer);
		}

#endif

		[Test]
		public void DeserializeDefault ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Write (_serializedDefault, 0, _serializedDefault.Length);
			ms.Position = 0;

			BinaryFormatter bf = new BinaryFormatter ();
			Comparer<int> c = (Comparer<int>) bf.Deserialize (ms);
			Assert.IsNotNull (c);
		}

		private static readonly byte [] _serializedDefault = new byte [] {
			0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x00,
			0x89, 0x01, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x43, 0x6f,
			0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x73, 0x2e, 0x47,
			0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x2e, 0x47, 0x65, 0x6e, 0x65,
			0x72, 0x69, 0x63, 0x43, 0x6f, 0x6d, 0x70, 0x61, 0x72, 0x65, 0x72,
			0x60, 0x31, 0x5b, 0x5b, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e,
			0x49, 0x6e, 0x74, 0x33, 0x32, 0x2c, 0x20, 0x6d, 0x73, 0x63, 0x6f,
			0x72, 0x6c, 0x69, 0x62, 0x2c, 0x20, 0x56, 0x65, 0x72, 0x73, 0x69,
			0x6f, 0x6e, 0x3d, 0x32, 0x2e, 0x30, 0x2e, 0x30, 0x2e, 0x30, 0x2c,
			0x20, 0x43, 0x75, 0x6c, 0x74, 0x75, 0x72, 0x65, 0x3d, 0x6e, 0x65,
			0x75, 0x74, 0x72, 0x61, 0x6c, 0x2c, 0x20, 0x50, 0x75, 0x62, 0x6c,
			0x69, 0x63, 0x4b, 0x65, 0x79, 0x54, 0x6f, 0x6b, 0x65, 0x6e, 0x3d,
			0x62, 0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36, 0x31, 0x39, 0x33,
			0x34, 0x65, 0x30, 0x38, 0x39, 0x5d, 0x5d, 0x00, 0x00, 0x00, 0x00,
			0x0b };
	}
}

#endif

