//
// XmlSerializer.cs
//
// Authors:
//   Rolf Bjarne Kvinge (RKvinge@novell.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

//
// This class is just a stub of the same class in System.Xml/System.Xml.Serialization
//

using System;
using System.IO;
namespace System.Xml.Serialization {
	public class XmlSerializer {
		public XmlSerializer ()
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer (Type type)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer (XmlTypeMapping xmlTYpeMapping)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer (Type type, Type [] extraTypes)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer (Type type, string defaultNamespace)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer (Type type, XmlAttributeOverrides overrides)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer (Type type, XmlRootAttribute root)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer (Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer (Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace, object location, object evidence)
		{
			throw new NotImplementedException ();
		}
		public bool CanDeserializer (XmlReader xmlReader)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializationReader CreateReader ()
		{
			throw new NotImplementedException ();
		}
		public XmlSerializationWriter CreateWriter ()
		{
			throw new NotImplementedException ();
		}
		public object Deserialize (Stream stream)
		{
			throw new NotImplementedException ();
		}
		public object Deserialize (XmlSerializationReader reader)
		{
			throw new NotImplementedException ();
		}
		public object Deserialize (TextReader textReader)
		{
			throw new NotImplementedException ();
		}
		public object Deserialize (XmlReader xmlReader)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer [] FromMappings (XmlMapping [] mappings)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer [] FromMappings (XmlMapping [] mappings, Type type)
		{
			throw new NotImplementedException ();
		}
		public XmlSerializer [] FromTypes (Type [] types)
		{
			throw new NotImplementedException ();
		}
		protected virtual void Serialize (object o, XmlSerializationWriter writer)
		{
			throw new NotImplementedException ();
		}
		public void Serialize (Stream stream, object o)
		{
			throw new NotImplementedException ();
		}
		public void Serialize (TextWriter textWriter, object o)
		{
			throw new NotImplementedException ();
		}
		public void Serialize (XmlWriter xmlWriter, object o)
		{
			throw new NotImplementedException ();
		}
		public void Serialize (TextWriter textWriter, object o, XmlSerializerNamespaces namespaces)
		{
			throw new NotImplementedException ();
		}
		public void Serialize (XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces)
		{
			throw new NotImplementedException ();
		}
		public void Serialize (Stream stream, object o, XmlSerializerNamespaces namespaces)
		{
			throw new NotImplementedException ();
		}
	}
}