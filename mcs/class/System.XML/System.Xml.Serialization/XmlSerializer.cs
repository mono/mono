//
// XmlSerializer.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Text;

namespace System.Xml.Serialization
{

	public class XmlSerializer
	{
		internal const string WsdlNamespace = "http://schemas.xmlsoap.org/wsdl/";
		internal const string EncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";

#region Fields

		XmlMapping typeMapping;

#endregion // Fields

#region Constructors

		protected XmlSerializer ()
		{
		}

		public XmlSerializer (Type type)
			: this (type, null, null, null, null)
		{
		}

		public XmlSerializer (XmlTypeMapping xmlTypeMapping)
		{
			typeMapping = xmlTypeMapping;
		}

		internal XmlSerializer (XmlMapping mapping)
		{
			typeMapping = mapping;
		}

		public XmlSerializer (Type type, string defaultNamespace)
			: this (type, null, null, null, defaultNamespace)
		{
		}

		public XmlSerializer (Type type, Type[] extraTypes)
			: this (type, null, extraTypes, null, null)
		{
		}

		public XmlSerializer (Type type, XmlAttributeOverrides overrides)
			: this (type, overrides, null, null, null)
		{
		}

		public XmlSerializer (Type type, XmlRootAttribute root)
			: this (type, null, null, root, null)
		{
		}

		public XmlSerializer (Type type,
			XmlAttributeOverrides overrides,
			Type [] extraTypes,
			XmlRootAttribute root,
			string defaultNamespace)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			XmlReflectionImporter importer = new XmlReflectionImporter (overrides, defaultNamespace);

			if (extraTypes != null) 
			{
				foreach (Type intype in extraTypes)
					importer.IncludeType (intype);
			}

			typeMapping = importer.ImportTypeMapping (type, root, defaultNamespace);
		}


#endregion // Constructors

#region Events

		private XmlAttributeEventHandler onUnknownAttribute;
		private XmlElementEventHandler onUnknownElement;
		private XmlNodeEventHandler onUnknownNode;
		private UnreferencedObjectEventHandler onUnreferencedObject;

		public event XmlAttributeEventHandler UnknownAttribute 
		{
			add { onUnknownAttribute += value; } remove { onUnknownAttribute -= value; }
		}

		public event XmlElementEventHandler UnknownElement 
		{
			add { onUnknownElement += value; } remove { onUnknownElement -= value; }
		}

		public event XmlNodeEventHandler UnknownNode 
		{
			add { onUnknownNode += value; } remove { onUnknownNode -= value; }
		}

		public event UnreferencedObjectEventHandler UnreferencedObject 
		{
			add { onUnreferencedObject += value; } remove { onUnreferencedObject -= value; }
		}


		internal virtual void OnUnknownAttribute (XmlAttributeEventArgs e) 
		{
			if (onUnknownAttribute != null) onUnknownAttribute(this, e);
		}

		internal virtual void OnUnknownElement (XmlElementEventArgs e) 
		{
			if (onUnknownElement != null) onUnknownElement(this, e);
		}

		internal virtual void OnUnknownNode (XmlNodeEventArgs e) 
		{
			if (onUnknownNode != null) onUnknownNode(this, e);
		}

		internal virtual void OnUnreferencedObject (UnreferencedObjectEventArgs e) 
		{
			if (onUnreferencedObject != null) onUnreferencedObject(this, e);
		}


#endregion // Events

#region Methods

		public virtual bool CanDeserialize (XmlReader xmlReader)
		{
			xmlReader.MoveToContent	();
			if (typeMapping is XmlMembersMapping) 
				return true;
			else
				return ((XmlTypeMapping)typeMapping).ElementName == xmlReader.LocalName;
		}

		protected virtual XmlSerializationReader CreateReader ()
		{
			// Must be implemented in derived class
			throw new NotImplementedException ();
		}

		protected virtual XmlSerializationWriter CreateWriter ()
		{
			// Must be implemented in derived class
			throw new NotImplementedException ();
		}

		public object Deserialize (Stream stream)
		{
			XmlTextReader xmlReader = new XmlTextReader(stream);
			return Deserialize(xmlReader);
		}

		public object Deserialize (TextReader textReader)
		{
			XmlTextReader xmlReader = new XmlTextReader(textReader);
			return Deserialize(xmlReader);
		}

		public object Deserialize (XmlReader xmlReader)
		{
			XmlSerializationReader xsReader;
			if (typeMapping == null)
				xsReader = CreateReader ();
			else
				xsReader = new XmlSerializationReaderInterpreter (typeMapping);
				
			xsReader.Initialize (xmlReader, this);
			return Deserialize (xsReader);
		}

		protected virtual object Deserialize (XmlSerializationReader reader)
		{
			if (typeMapping != null)
			{
				XmlSerializationReaderInterpreter rd = reader as XmlSerializationReaderInterpreter;
				if (rd == null) throw new InvalidOperationException ();
				return rd.ReadObject ();
			}
			else
				// Must be implemented in derived class
				throw new NotImplementedException ();
		}

		public static XmlSerializer [] FromMappings (XmlMapping	[] mappings)
		{
			XmlSerializer [] sers = new XmlSerializer [mappings.Length];
			for (int n=0; n<mappings.Length; n++)
				if (mappings[n] != null)
					sers[n] = new XmlSerializer (mappings[n]);
					
			return sers;
		}

		public static XmlSerializer [] FromTypes (Type [] mappings)
		{
			XmlSerializer [] sers = new XmlSerializer [mappings.Length];
			for (int n=0; n<mappings.Length; n++)
				sers[n] = new XmlSerializer (mappings[n]);
			return sers;
		}

		protected virtual void Serialize (object o, XmlSerializationWriter writer)
		{
			if (typeMapping != null)
			{
				XmlSerializationWriterInterpreter wr = writer as XmlSerializationWriterInterpreter;
				if (wr == null) throw new InvalidOperationException ();
				wr.WriteObject (o);
			}
			else
				// Must be implemented in derived class
				throw new NotImplementedException ();
		}

		public void Serialize (Stream stream, object o)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (stream, System.Text.Encoding.Default);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, null);
		}

		public void Serialize (TextWriter textWriter, object o)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (textWriter);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, null);
		}

		public void Serialize (XmlWriter xmlWriter, object o)
		{
			Serialize (xmlWriter, o, null);
		}

		public void Serialize (Stream stream, object o, XmlSerializerNamespaces	namespaces)
		{
			XmlTextWriter xmlWriter	= new XmlTextWriter (stream, System.Text.Encoding.Default);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, namespaces);
		}

		public void Serialize (TextWriter textWriter, object o, XmlSerializerNamespaces	namespaces)
		{
			XmlTextWriter xmlWriter	= new XmlTextWriter (textWriter);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, namespaces);
			xmlWriter.Flush();
		}

		public void Serialize (XmlWriter writer, object o, XmlSerializerNamespaces namespaces)
		{
			XmlSerializationWriter xsWriter;
			
			if (typeMapping == null)
				xsWriter = CreateWriter ();
			else
				xsWriter = new XmlSerializationWriterInterpreter (typeMapping);
				
			if (namespaces == null || namespaces.Count == 0)
			{
				namespaces = new XmlSerializerNamespaces ();
				namespaces.Add ("xsd", XmlSchema.Namespace);
				namespaces.Add ("xsi", XmlSchema.InstanceNamespace);
			}
			
			xsWriter.Initialize (writer, namespaces);
			Serialize (o, xsWriter);
			writer.Flush ();
		}
#endregion // Methods
	}
}
