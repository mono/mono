//
// AnotherSerializable.cs : Serializable Class used to test types from other
// assemblies for resources tests
//
// Author:
//	Gary Barnett (gary.barnett.mono@gmail.com)
// 
// Copyright (C) Gary Barnett (2012)
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

using System;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DummyAssembly {
	[SerializableAttribute]
	public class AnotherSerializable : ISerializable {
		public string name;
		public string value;

		public AnotherSerializable ()
		{
		}

		public AnotherSerializable (string name, string value)
		{
			this.name = name;
			this.value = value;
		}

		public AnotherSerializable (SerializationInfo info, StreamingContext ctxt)
		{
			name = (string) info.GetValue ("sername", typeof (string));
			value = (String) info.GetValue ("servalue", typeof (string));
		}

		public AnotherSerializable (Stream stream)
		{
			BinaryFormatter bFormatter = new BinaryFormatter ();
			AnotherSerializable deser = (AnotherSerializable) bFormatter.Deserialize (stream);
			stream.Close ();

			name = deser.name;
			value = deser.value;
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
			AnotherSerializable o = obj as AnotherSerializable;
			if (o == null)
				return false;
			return this.name.Equals (o.name) && this.value.Equals (o.value);
		}
	}
}

