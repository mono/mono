using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace DummyAssembly {

    [SerializableAttribute]
    [TypeConverter (typeof (ConvertableConverter))]
    public class Convertable {
        protected string name;
        protected string value;

        public Convertable ()
        {
        }

        public Convertable (string name, string value)
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
            return String.Format ("{0}\t{1}",name, value);
        }

        public override bool Equals (object obj)
        {
            Convertable o = obj as Convertable;
            if (o == null)
                return false;
            return this.name.Equals (o.name) && this.value.Equals (o.value);
        }
    }

    class ConvertableConverter : TypeConverter {
        public ConvertableConverter ()
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

        public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.GetType() != typeof (string))
                throw new Exception ("value not string");

            string serialised = (string) value;

            string [] parts = serialised.Split ('\t');

            if (parts.Length != 2)
                throw new Exception ("string in incorrect format");

            Convertable convertable = new Convertable (parts [0], parts [1]);
            return convertable;
        }

        public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof (String)) {
                return base.ConvertTo (context, culture, value, destinationType);
            }

            return ((Convertable) value).ToString ();
        }
    }

}
