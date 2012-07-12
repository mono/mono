using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace MonoTests.System.Resources {
	class notserializable {
        public object test;
        public notserializable ()
        {

        }
    }

    [SerializableAttribute]
    public class serializable : ISerializable {
        public string name;
        public string value;

        public serializable ()
        {
        }

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

        public serializable (Stream stream)
        {
            BinaryFormatter bFormatter = new BinaryFormatter ();
            serializable deser = (serializable) bFormatter.Deserialize (stream);
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
            serializable o = obj as serializable;
            if (o == null)
                return false;
            return this.name.Equals (o.name) && this.value.Equals (o.value);
        }
    }
    
    [SerializableAttribute]
    public class serializableSubClass : serializable {
        public serializableSubClass ()
        {
        }

        public serializableSubClass (SerializationInfo info, StreamingContext ctxt)
            : base (info, ctxt)
        {
        }

        public serializableSubClass (Stream stream)
        {
            BinaryFormatter bFormatter = new BinaryFormatter ();
            serializableSubClass deser = (serializableSubClass) bFormatter.Deserialize (stream);
            stream.Close ();

            name = deser.name;
            value = deser.value;
        }
    }

    [SerializableAttribute]
    [TypeConverter (typeof (ThisAssemblyConvertableConverter))]
    public class ThisAssemblyConvertable {
        protected string name;
        protected string value;

        public ThisAssemblyConvertable ()
        {
        }

        public ThisAssemblyConvertable (string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public void GetObjectData (SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue ("sername", name);
            info.AddValue ("servalue", value);
        }

        public override string ToString ()
        {
            return String.Format ("{0}\t{1}", name, value);
        }

        public override bool Equals (object obj)
        {
            ThisAssemblyConvertable o = obj as ThisAssemblyConvertable;
            if (o == null)
                return false;
            return this.name.Equals (o.name) && this.value.Equals (o.value);
        }
    }

    class ThisAssemblyConvertableConverter : TypeConverter {
        public ThisAssemblyConvertableConverter ()
        {
        }

        public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof (string);
        }

        public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof (string);
        }

        public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType () != typeof (string))
                throw new Exception ("value not string");

            string serialised = (string) value;

            string [] parts = serialised.Split ('\t');

            if (parts.Length != 2)
                throw new Exception ("string in incorrect format");

            ThisAssemblyConvertable convertable = new ThisAssemblyConvertable (parts [0], parts [1]);
            return convertable;
        }

        public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType != typeof (String)) {
                return base.ConvertTo (context, culture, value, destinationType);
            }

            return ((ThisAssemblyConvertable) value).ToString ();
        }
    }
}
