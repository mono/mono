//
// Convertable.cs : Class with type converter used to test types from other
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

