//
// XmlSerializerImplementation.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
//

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
using System;
using System.Collections;
using System.Xml.Serialization;

using NUnit.Framework;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlSerializerImplementationTests
	{
		class MyImplementation : XmlSerializerImplementation
		{
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DefaultReder ()
		{
			MyImplementation impl = new MyImplementation ();
			Assert.IsNull (impl.Reader, "#1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DefaultRederMethods ()
		{
			MyImplementation impl = new MyImplementation ();
			Assert.IsNull (impl.ReadMethods, "#2");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DefaultWriter ()
		{
			MyImplementation impl = new MyImplementation ();
			Assert.IsNull (impl.Writer, "#3");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DefaultWriteMethods ()
		{
			MyImplementation impl = new MyImplementation ();
			Assert.IsNull (impl.WriteMethods, "#4");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DefaultTypedSerializers ()
		{
			MyImplementation impl = new MyImplementation ();
			Assert.IsNull (impl.TypedSerializers, "#5");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CanSerialize ()
		{
			MyImplementation impl = new MyImplementation ();
			Assert.IsTrue (impl.CanSerialize (typeof (int)), "#1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void GetSerializer ()
		{
			MyImplementation impl = new MyImplementation ();
			Assert.IsNull (impl.GetSerializer (typeof (int)), "#1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void GetSerializer2 ()
		{
			MyImplementation2 impl = new MyImplementation2 ();
			impl.TypedSerializers [typeof (int)] = new XmlSerializer (typeof (int));
			XmlSerializer ser = impl.GetSerializer (typeof (int));
		}

		class MyImplementation2 : XmlSerializerImplementation
		{
			Hashtable serializers = new Hashtable ();

			public override Hashtable TypedSerializers {
				get { return serializers; }
			}
		}
	}
}
#endif
