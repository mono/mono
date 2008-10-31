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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// ResXFileRefTest.cs: Unit Tests for ResXFileRef.
//
// Authors:
//	Andreia Gaita	(avidigal@novell.com)
//

#if NET_2_0

using System;
using System.Resources;
using System.Runtime.Serialization;
using System.Collections;

using NUnit.Framework;
namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResXDataNodeTest : MonoTests.System.Windows.Forms.TestHelper
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorEx1 ()
		{
			ResXDataNode d = new ResXDataNode (null, (object)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorEx2 ()
		{
			ResXDataNode d = new ResXDataNode (null, (ResXFileRef) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorEx3 ()
		{
			ResXDataNode d = new ResXDataNode ("", (object) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorEx4 ()
		{
			ResXDataNode d = new ResXDataNode ("", (ResXFileRef) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorEx5 ()
		{
			ResXDataNode d = new ResXDataNode ("", new ResXFileRef ("filename", "typename"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConstructorEx6 ()
		{
			ResXDataNode d = new ResXDataNode ("name", new notserializable ());
		}

		[Test]
		public void WriteRead1 ()
		{
			ResXResourceWriter rw = new ResXResourceWriter ("resx.resx");
			serializable ser = new serializable ("aaaaa", "bbbbb");
			ResXDataNode dn = new ResXDataNode ("test", ser);
			dn.Comment = "comment";
			rw.AddResource (dn);
			rw.Close ();

			bool found = false;
			ResXResourceReader rr = new ResXResourceReader ("resx.resx");
			IDictionaryEnumerator en = rr.GetEnumerator ();
			while (en.MoveNext ()) {
				serializable o = ((DictionaryEntry) en.Current).Value as serializable;
				if (o != null) {
					found = true;
					Assert.AreEqual (ser, o, "#A1");
				}

			}
			rr.Close ();

			Assert.IsTrue (found, "#A2 - Serialized object not found on resx");
		}
	}

	class notserializable
	{
		public object test;
		public notserializable ()
		{

		}
	}

	[SerializableAttribute]
	public class serializable : ISerializable
	{
		string name;
		string value;

		public serializable (string name, string value)
		{
			this.name = name;
			this.value = value;
		}

		public serializable (SerializationInfo info, StreamingContext ctxt)
		{
			name = (string) info.GetValue ("sername", typeof (string));
			value = (String) info.GetValue ("servalue", typeof (string));
		}

		public void GetObjectData (SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue ("sername", name);
			info.AddValue ("servalue", value);
		}

		public override string ToString ()
		{
			return String.Format ("name={0};value={1}", this.name, this.value);
		}

		public override bool Equals (object obj)
		{
			serializable o = obj as serializable;
			if (o == null)
				return false;
			return this.name.Equals(o.name) && this.value.Equals(o.value);
		}
	}
}
#endif

