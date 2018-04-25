//
// System.ServiceModel.AddressHeader.cs
//
// Authors:
//	Duncan Mak (duncan@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005,2010 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;
using System.Xml.Schema;

namespace System.ServiceModel.Channels
{
	public abstract class AddressHeader
	{
		protected AddressHeader () {}

		public static AddressHeader CreateAddressHeader (object value)
		{
			return new DefaultAddressHeader (value);
		}

		public static AddressHeader CreateAddressHeader (object value, XmlObjectSerializer serializer)
		{
			return new DefaultAddressHeader (value, serializer);
		}

		public static AddressHeader CreateAddressHeader (string name, string ns, object value)
		{
			return new DefaultAddressHeader (name, ns, value);
		}

		public static AddressHeader CreateAddressHeader (string name, string ns, object value, 
								 XmlObjectSerializer serializer)
		{
			if (serializer == null)
				throw new ArgumentNullException ("serializer");
			return new DefaultAddressHeader (name, ns, value, serializer);
		}

		public override bool Equals (object obj)
		{
			AddressHeader o = obj as AddressHeader;

			if (o == null)
				return false;

			return o.Name == this.Name && o.Namespace == this.Namespace; 
		}

		public virtual XmlDictionaryReader GetAddressHeaderReader ()
		{
			var sw = new StringWriter ();
			var s = new XmlWriterSettings () { OmitXmlDeclaration = true };
			var xw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw, s));
			WriteAddressHeader (xw);
			xw.Close ();
			return XmlDictionaryReader.CreateDictionaryReader (XmlReader.Create (new StringReader (sw.ToString ())));
		}

		public override int GetHashCode ()
		{
			return this.Name.GetHashCode () + this.Namespace.GetHashCode ();
		}

		public T GetValue<T> ()
		{
			return GetValue<T> (new DataContractSerializer (typeof (T)));
		}

		public T GetValue<T> (XmlObjectSerializer serializer)
		{
			return (T) serializer.ReadObject (GetAddressHeaderReader ());
		}

		protected abstract void OnWriteAddressHeaderContents (XmlDictionaryWriter writer);
		protected virtual void OnWriteStartAddressHeader (XmlDictionaryWriter writer)
		{
			if (Name != null && Namespace != null)
				writer.WriteStartElement (Name, Namespace);
		}

		public MessageHeader ToMessageHeader ()
		{
			throw new NotImplementedException ();			
		}

		public void WriteAddressHeader (XmlDictionaryWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer is null");
			
			this.WriteStartAddressHeader (writer);
			this.WriteAddressHeaderContents (writer);
			if (Name != null && Namespace != null)
				writer.WriteEndElement ();
		}

		public void WriteAddressHeader (XmlWriter writer)
		{
			this.WriteAddressHeader (XmlDictionaryWriter.CreateDictionaryWriter (writer));
		}

		public void WriteAddressHeaderContents (XmlDictionaryWriter writer)
		{
			this.OnWriteAddressHeaderContents (writer);
		}

		public void WriteStartAddressHeader (XmlDictionaryWriter writer)
		{
			this.OnWriteStartAddressHeader (writer);
		}

		public abstract string Name { get; }
		public abstract string Namespace { get; }

		internal class DefaultAddressHeader : AddressHeader
		{
			string name, ns;
			XmlObjectSerializer formatter;
			object value;

			internal DefaultAddressHeader (object value)
				: this (null, null, value) {}

			
			internal DefaultAddressHeader (object value, XmlObjectSerializer formatter)
				: this (null, null, value, formatter)
			{
			}

			internal DefaultAddressHeader (string name, string ns, object value)
				: this (name, ns, value, null) {}
			
			internal DefaultAddressHeader (string name, string ns, object value, XmlObjectSerializer formatter)
			{
				if (formatter == null)
					formatter = value != null ? new DataContractSerializer (value.GetType ()) : null;
				this.name = name;
				this.ns = ns;
				this.formatter = formatter;
				this.value = value;
			}

			public override string Name {
				get { return name; }
			}

			public override string Namespace {
				get { return ns; }
			}

			protected override void OnWriteAddressHeaderContents (XmlDictionaryWriter writer)
			{
				if (value == null)
					writer.WriteAttributeString ("i", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
				else
					this.formatter.WriteObject (writer, value);
			}
		}
		
	}
}