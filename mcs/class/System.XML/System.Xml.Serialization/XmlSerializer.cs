//
// XmlSerializer.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System;

namespace System.Xml.Serialization
{	
	/// <summary>
	/// Summary description for XmlSerializer.
	/// </summary>
	public class XmlSerializer
	{
		[MonoTODO]
		protected XmlSerializer ()
		{
		}

		[MonoTODO]
		public XmlSerializer (Type type)
		{
		}

		[MonoTODO]
		public XmlSerializer (XmlTypeMapping xmltypemapping)
		{
		}

		[MonoTODO]
		public XmlSerializer (Type type, string defaultNamespace)
		{
		}

		[MonoTODO]
		public XmlSerializer (Type type, Type[] extraTypes)
		{
		}

		[MonoTODO]
		public XmlSerializer (Type type, XmlAttributeOverrides overrides)
		{
		}

		[MonoTODO]
		public XmlSerializer (Type type, XmlRootAttribute root)
		{
		}

		[MonoTODO]
		public XmlSerializer (Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace)
		{
		}


		
		[MonoTODO]
		public event XmlAttributeEventHandler UnknownAttribute;
		[MonoTODO]
		public event XmlElementEventHandler UnknownElement;
		[MonoTODO]
		public event XmlNodeEventHandler UnknownNode;
		[MonoTODO]
		public event UnreferencedObjectEventHandler UnreferencedObject;
		

		[MonoTODO]
		public virtual bool CanDeserialize (XmlReader xmlReader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object Deserialize (Stream stream)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public object Deserialize (TextReader textReader)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public object Deserialize (XmlReader xmlReader)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Serialize (Stream stream, object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Serialize (TextWriter textWriter, object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Serialize (XmlWriter xmlWriter, object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Serialize (Stream stream, object o, XmlSerializerNamespaces namespaces)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Serialize (TextWriter textWriter, object o, XmlSerializerNamespaces namespaces)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Serialize (XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces)
		{
			throw new NotImplementedException ();
		}
	}
}
