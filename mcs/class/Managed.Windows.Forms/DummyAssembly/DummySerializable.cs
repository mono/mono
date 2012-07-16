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

