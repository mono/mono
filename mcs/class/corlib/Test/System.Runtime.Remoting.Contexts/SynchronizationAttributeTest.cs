//
// SynchronizationAttributeTest.cs - Unit tests for 
//	System.Runtime.Remoting.Contexts.SynchronizationAttribute
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
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting.Contexts {

	[TestFixture]
	public class SynchronizationAttributeTest {

		[Test]
		public void Constructor_Default ()
		{
			SynchronizationAttribute sa = new SynchronizationAttribute ();
			Assert.IsFalse (sa.IsReEntrant, "IsReEntrant");
			Assert.IsFalse (sa.Locked, "Locked");
		}

		[Test]
		public void Constructor_Bool ()
		{
			SynchronizationAttribute sa = new SynchronizationAttribute (true);
			Assert.IsTrue (sa.IsReEntrant, "IsReEntrant");
			Assert.IsFalse (sa.Locked, "Locked");
		}

		[Test]
		public void Constructor_Int ()
		{
			SynchronizationAttribute sa = new SynchronizationAttribute (SynchronizationAttribute.NOT_SUPPORTED);
			Assert.IsFalse (sa.IsReEntrant, "IsReEntrant");
			Assert.IsFalse (sa.Locked, "Locked");
		}

		[Test]
		public void Constructor_IntBool ()
		{
			SynchronizationAttribute sa = new SynchronizationAttribute (SynchronizationAttribute.NOT_SUPPORTED, true);
			Assert.IsTrue (sa.IsReEntrant, "IsReEntrant");
			Assert.IsFalse (sa.Locked, "Locked");
		}

		[Test]
	    public void SetLocked()
		{
			SynchronizationAttribute sa = new SynchronizationAttribute(SynchronizationAttribute.REQUIRES_NEW);
			sa.Locked = true;
			Assert.IsTrue(sa.Locked, "Locked");
			sa.Locked = false;
			Assert.IsFalse(sa.Locked, "Locked");

			sa.Locked = true;
			Assert.IsTrue(sa.Locked, "Locked");
			sa.Locked = true;
			Assert.IsTrue(sa.Locked, "Locked");
			sa.Locked = false;
			Assert.IsFalse(sa.Locked, "Locked");
		}

		[Test]
		public void SerializationRoundtrip ()
		{
			SynchronizationAttribute sa = new SynchronizationAttribute (SynchronizationAttribute.NOT_SUPPORTED, true);
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, sa);

			ms.Position = 0;
			SynchronizationAttribute clone = (SynchronizationAttribute) bf.Deserialize (ms);

			Assert.IsTrue (sa.IsReEntrant, "IsReEntrant");
			Assert.IsFalse (sa.Locked, "Locked");
		}

		static private byte[] serialized_sync_attr = {
			0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x00, 0x39, 0x53, 0x79, 0x73, 0x74, 0x65,
			0x6D, 0x2E, 0x52, 0x75, 0x6E, 0x74, 0x69, 0x6D, 0x65, 0x2E, 0x52, 0x65, 0x6D, 0x6F,
			0x74, 0x69, 0x6E, 0x67, 0x2E, 0x43, 0x6F, 0x6E, 0x74, 0x65, 0x78, 0x74, 0x73, 0x2E,
			0x53, 0x79, 0x6E, 0x63, 0x68, 0x72, 0x6F, 0x6E, 0x69, 0x7A, 0x61, 0x74, 0x69, 0x6F,
			0x6E, 0x41, 0x74, 0x74, 0x72, 0x69, 0x62, 0x75, 0x74, 0x65, 0x04, 0x00, 0x00, 0x00,
			0x0B, 0x5F, 0x62, 0x52, 0x65, 0x45, 0x6E, 0x74, 0x72, 0x61, 0x6E, 0x74, 0x07, 0x5F,
			0x66, 0x6C, 0x61, 0x76, 0x6F, 0x72, 0x0D, 0x41, 0x74, 0x74, 0x72, 0x69, 0x62, 0x75,
			0x74, 0x65, 0x4E, 0x61, 0x6D, 0x65, 0x1E, 0x43, 0x6F, 0x6E, 0x74, 0x65, 0x78, 0x74,
			0x41, 0x74, 0x74, 0x72, 0x69, 0x62, 0x75, 0x74, 0x65, 0x2B, 0x41, 0x74, 0x74, 0x72,
			0x69, 0x62, 0x75, 0x74, 0x65, 0x4E, 0x61, 0x6D, 0x65, 0x00, 0x00, 0x01, 0x01, 0x01,
			0x08, 0x01, 0x01, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x0F, 0x53, 0x79,
			0x6E, 0x63, 0x68, 0x72, 0x6F, 0x6E, 0x69, 0x7A, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x09,
			0x02, 0x00, 0x00, 0x00, 0x0B
		};

		[Test]
		public void DeserializeKnownValue ()
		{
			MemoryStream ms = new MemoryStream (serialized_sync_attr);
			BinaryFormatter bf = new BinaryFormatter ();
			SynchronizationAttribute sa = (SynchronizationAttribute) bf.Deserialize (ms);
			Assert.IsTrue (sa.IsReEntrant, "IsReEntrant");
			Assert.IsFalse (sa.Locked, "Locked");
		}
	}
}
