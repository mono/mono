// 
// System.Xml.Serialization.XmlSerializerFactory.cs 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Novell, Inc., 2004
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
using System.Collections;
using System.Security.Policy;

namespace System.Xml.Serialization 
{
	public class XmlSerializerFactory
	{
		static Hashtable serializersBySource = new Hashtable ();
		
		public XmlSerializerFactory ()
		{
		}

		public XmlSerializer CreateSerializer (Type type)
		{
			return CreateSerializer (type, null, null, null, null);
		}

		public XmlSerializer CreateSerializer (XmlTypeMapping xmlTypeMapping)
		{
			lock (serializersBySource) 
			{
				XmlSerializer ser = (XmlSerializer) serializersBySource [xmlTypeMapping.Source];
				if (ser == null) {
					ser = new XmlSerializer (xmlTypeMapping);
					serializersBySource [xmlTypeMapping.Source] = ser;
				}
				return ser;
			}
		}

		public XmlSerializer CreateSerializer (Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace, string location)
		{
			return CreateSerializer (type, overrides, extraTypes, root, defaultNamespace, location, null);
		}

		public XmlSerializer CreateSerializer (Type type, string defaultNamespace)
		{
			return CreateSerializer (type, null, null, null, defaultNamespace);
		}

		public XmlSerializer CreateSerializer (Type type, Type[] extraTypes)
		{
			return CreateSerializer (type, null, extraTypes, null, null);
		}

		public XmlSerializer CreateSerializer (Type type, XmlAttributeOverrides overrides)
		{
			return CreateSerializer (type, overrides, null, null, null);
		}

		public XmlSerializer CreateSerializer (Type type, XmlRootAttribute root)
		{
			return CreateSerializer (type, null, null, root, null);
		}
		
		public XmlSerializer CreateSerializer (Type type, 
			XmlAttributeOverrides overrides, 
			Type[] extraTypes, 
			XmlRootAttribute root, 
			string defaultNamespace)
		{
			XmlTypeSerializationSource source = new XmlTypeSerializationSource (type, root, overrides, defaultNamespace, extraTypes);
			lock (serializersBySource) 
			{
				XmlSerializer ser = (XmlSerializer) serializersBySource [source];
				if (ser == null) {
					ser = new XmlSerializer (type, overrides, extraTypes, root, defaultNamespace);
					serializersBySource [ser.Mapping.Source] = ser;
				}
				return ser;
			}
		}

		[MonoTODO]
		public XmlSerializer CreateSerializer (Type type, 
			XmlAttributeOverrides overrides, 
			Type[] extraTypes, 
			XmlRootAttribute root, 
			string defaultNamespace,
			string location,
			Evidence evidence)
		{
			throw new NotImplementedException ();
		}
	}
}

