//
// System.Xml.Serialization.XmlSerializationReader.cs
//
// Authors:
// 	Tim Coleman (tim@timcoleman.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Xml;

namespace System.Xml.Serialization {
	public abstract class XmlSerializationReader {

		#region Fields

		XmlDocument document;
		XmlReader reader;
		ArrayList fixups;
		ArrayList collFixups;
		Hashtable readCallbacks;
		Hashtable typesCallbacks;
		ArrayList noIDTargets;
		Hashtable targets;

		string w3SchemaNS;
		string w3SchemaNS2000;
		string w3SchemaNS1999;
		string w3InstanceNS;
		string w3InstanceNS2000;
		string w3InstanceNS1999;
		string soapNS;
		string schema;
		string wsdlNS;
		string wsdlArrayType;
		string nullX;
		string nil;
		string typeX;
		string arrayType;
		string anyType;
		#endregion

		private void Initialize ()
		{
			w3SchemaNS = reader.NameTable.Add ("http://www.w3.org/2001/XMLSchema");
			w3SchemaNS2000 = reader.NameTable.Add ("http://www.w3.org/2000/10/XMLSchema");
			w3SchemaNS1999 = reader.NameTable.Add ("http://www.w3.org/1999/XMLSchema");
			w3InstanceNS = reader.NameTable.Add ("http://www.w3.org/2001/XMLSchema-instance");
			w3InstanceNS2000 = reader.NameTable.Add ("http://www.w3.org/2000/10/XMLSchema-instance");
			w3InstanceNS1999 = reader.NameTable.Add ("http://www.w3.org/1999/XMLSchema-instance");
			soapNS = reader.NameTable.Add ("http://schemas.xmlsoap.org/soap/encoding/");
			schema = reader.NameTable.Add ("schema");
			wsdlNS = reader.NameTable.Add ("http://schemas.xmlsoap.org/wsdl/");
			wsdlArrayType = reader.NameTable.Add ("arrayType");
			nullX = reader.NameTable.Add ("null");
			nil = reader.NameTable.Add ("nil");
			typeX = reader.NameTable.Add ("type");
			arrayType = reader.NameTable.Add ("arrayType");
			anyType = reader.NameTable.Add ("anyType");
			InitIDs ();
		}
			
		private ArrayList EnsureArrayList (ArrayList list)
		{
			if (list == null)
				list = new ArrayList ();
			return list;
		}
		
		private Hashtable EnsureHashtable (Hashtable hash)
		{
			if (hash == null)
				hash = new Hashtable ();
			return hash;
		}
		
		protected XmlSerializationReader ()
		{
		}

		protected XmlDocument Document
		{
			get {
				if (document == null)
					document = new XmlDocument (reader.NameTable);

				return document;
			}
		}

		protected XmlReader Reader {
			get { return reader; }
		}

		#region Methods

		protected void AddFixup (CollectionFixup fixup)
		{
			collFixups = EnsureArrayList (collFixups);
			collFixups.Add(fixup);
		}

		protected void AddFixup (Fixup fixup)
		{
			fixups = EnsureArrayList (fixups);
			fixups.Add(fixup);
		}

		protected void AddReadCallback (string name, string ns, Type type, XmlSerializationReadCallback read)
		{
			XmlNameTable nt = reader.NameTable;
			XmlQualifiedName xqn = new XmlQualifiedName (nt.Add (name), nt.Add (ns));
			readCallbacks = EnsureHashtable (readCallbacks);
			readCallbacks.Add (xqn, read);
			typesCallbacks = EnsureHashtable (typesCallbacks);
			typesCallbacks.Add (xqn, type);
		}

		protected void AddTarget (string id, object o)
		{
			if (id != null) {
				targets = EnsureHashtable (targets);
				if (targets [id] == null)
					targets.Add (id, o);
			} else {
				if (o != null)
					return;
				noIDTargets = EnsureArrayList (noIDTargets);
				noIDTargets.Add (o);
			}
		}

		private string CurrentTag ()
		{
			switch (reader.NodeType) {
			case XmlNodeType.None:
				return String.Format ("<{0} xmlns='{1}'>", reader.LocalName,
									   reader.NamespaceURI);
			case XmlNodeType.Attribute:
				return reader.Value;
			case XmlNodeType.Text:
				return "CDATA";
			case XmlNodeType.ProcessingInstruction:
				return "<--";
			case XmlNodeType.Entity:
				return "<?";
			case XmlNodeType.EndElement:
				return ">";
			default:
				return "(unknown)";
			}
		}

		protected Exception CreateAbstractTypeException (string name, string ns)
		{
			string message = "Error at " + name + " " + ns + ":" + CurrentTag ();
			return new InvalidOperationException (message);
		}

		protected Exception CreateInvalidCastException (Type type, object value)
		{
			string message = String.Format ("Cannot assign object of type {0} to an object of " +
							"type {1}.", value.GetType (), type);
			return new InvalidCastException (message);
		}

		protected Exception CreateReadOnlyCollectionException (string name)
		{
			string message = String.Format ("Could not serialize {0}. Default constructors are " +
							"required for collections and enumerators.", name);
			return new InvalidOperationException (message);
		}

		protected Exception CreateUnknownConstantException (string value, Type enumType)
		{
			string message = String.Format ("'{0}' is not a valid value for {1}.", value, enumType);
			return new InvalidOperationException (message);
		}

		protected Exception CreateUnknownNodeException ()
		{
			string message = "Unknown xml node -> " + CurrentTag ();
			return new InvalidOperationException (message);
		}

		protected Exception CreateUnknownTypeException (XmlQualifiedName type)
		{
			string message = "Unknown type " + type.Namespace + ":" + type.Name + " " + CurrentTag ();
			return new InvalidOperationException (message);
		}

		protected Array EnsureArrayIndex (Array a, int index, Type elementType)
		{
			if (a != null && index < a.Length)
				return a;

			int size;
			if (a == null) {
				size = 32;
			} else {
				size = a.Length * 2;
			}

			Array result = Array.CreateInstance (elementType, size);
			if (a != null)
				Array.Copy (a, result, index);

			return result;
		}

		[MonoTODO ("Implement")]
		protected void FixupArrayRefs (object fixup)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected int GetArrayLength (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		protected bool GetNullAttr ()
		{
			string na = reader.GetAttribute (nullX, w3InstanceNS);
			if (na == null) {
				na = reader.GetAttribute (nil, w3InstanceNS);
				if (na == null) {
					na = reader.GetAttribute (nullX, w3InstanceNS2000);
					if (na == null)
						na = reader.GetAttribute (nullX, w3InstanceNS1999);
				}
			}
			return (na != null);
		}

		[MonoTODO ("Implement")]
		protected object GetTarget (string id)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected XmlQualifiedName GetXsiType ()
		{
			throw new NotImplementedException ();
		}

		protected abstract void InitCallbacks ();
		protected abstract void InitIDs ();

		protected bool IsXmlnsAttribute (string name)
		{
			int length = name.Length;
			if (length < 5)
				return false;

			if (length == 5)
				return (name == "xmlns");

			return name.StartsWith ("xmlns:");
		}

		[MonoTODO ("Implement")]
		protected void ParseWsdlArrayType (XmlAttribute attr)
		{
			throw new NotImplementedException ();
		}

		protected XmlQualifiedName ReadElementQualifiedName ()
		{
			if (reader.IsEmptyElement) {
				reader.Skip();
				return ToXmlQualifiedName (String.Empty);
			}

			XmlQualifiedName xqn = ToXmlQualifiedName(reader.ReadString ());
			reader.ReadEndElement ();
			return xqn;
		}

		protected void ReadEndElement ()
		{
			while (reader.NodeType == XmlNodeType.Whitespace)
				reader.Skip ();

			if (reader.NodeType != XmlNodeType.None) {
				reader.ReadEndElement ();
			} else {
				reader.Skip ();
			}
		}

		protected bool ReadNull ()
		{
			if (!GetNullAttr ())
				return false;

			if (reader.IsEmptyElement) {
				reader.Skip();
				return true;
			}

			reader.ReadStartElement();
			while (reader.NodeType != XmlNodeType.EndElement)
				UnknownNode (null);
			ReadEndElement ();
			return true;
		}

		protected XmlQualifiedName ReadNullableQualifiedName ()
		{
			if (ReadNull ())
				return null;

			return ReadElementQualifiedName ();
		}

		protected string ReadNullableString ()
		{
			if (ReadNull ())
				return null;

			return reader.ReadElementString ();
		}

		protected bool ReadReference (out string fixupReference)
		{
			string href = reader.GetAttribute ("href");
			if (href == null) {
				fixupReference = null;
				return false;
			}

			if (href [0] != '#')
				throw new InvalidOperationException("href not found: " + href);

			fixupReference = href.Substring (1);
			if (!reader.IsEmptyElement) {
				reader.ReadStartElement ();
				ReadEndElement ();
			} else {
				reader.Skip ();
			}
			return true;
		}

		protected object ReadReferencedElement ()
		{
			return ReadReferencedElement (null, null);
		}

		protected object ReadReferencedElement (string name, string ns)
		{
			string unused;
			return ReadReferencingElement (name, ns, false, out unused);
		}

		protected void ReadReferencedElements ()
		{
			string unused;

			reader.MoveToContent();
			XmlNodeType nt = reader.NodeType;
			while (nt != XmlNodeType.EndElement && nt != XmlNodeType.None) {
				ReadReferencingElement (null, null, true, out unused);
				reader.MoveToContent ();
				nt = reader.NodeType;
			}
		}

		[MonoTODO ("Implement")]
		protected object ReadReferencingElement (out string fixupReference)
		{
			return ReadReferencingElement (null, null, false, out fixupReference);
		}

		protected object ReadReferencingElement (string name, string ns, out string fixupReference)
		{
			return ReadReferencingElement (name, ns, false, out fixupReference);
		}

		[MonoTODO]
		protected object ReadReferencingElement (string name,
							 string ns,
							 bool elementCanBeType,
							 out string fixupReference)
		{
			throw new NotImplementedException ();
		}

		protected IXmlSerializable ReadSerializable (IXmlSerializable serializable)
		{
			serializable.ReadXml (reader);
			return serializable;
		}

		protected string ReadString (string value)
		{
			if (value == null || value == String.Empty)
				return reader.ReadString ();

			return (value + reader.ReadString ());
		}

		[MonoTODO ("Implement")]
		protected object ReadTypedPrimitive (XmlQualifiedName type)
		{
			throw new NotImplementedException ();
		}

		protected XmlNode ReadXmlNode (bool wrapped)
		{
			return document.ReadNode (reader);
		}

		[MonoTODO ("Implement")]
		protected void Referenced (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Array ShrinkArray (Array a, int length, Type elementType, bool isNullable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected byte[] ToByteArrayBase64 (bool isNull)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static byte[] ToByteArrayBase64 (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected byte[] ToByteArrayHex (bool isNull)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static byte[] ToByteArrayHex (string value)
		{
			throw new NotImplementedException ();
		}

		protected static char ToChar (string value)
		{
			return XmlCustomFormatter.ToChar (value);
		}

		protected static DateTime ToDate (string value)
		{
			return XmlCustomFormatter.ToDate (value);
		}

		protected static DateTime ToDateTime (string value)
		{
			return XmlCustomFormatter.ToDateTime (value);
		}

		protected static long ToEnum (string value, Hashtable h, string typeName)
		{
			return XmlCustomFormatter.ToEnum (value, h, typeName, true);
		}

		protected static DateTime ToTime (string value)
		{
			return XmlCustomFormatter.ToTime (value);
		}

		protected static string ToXmlName (string value)
		{
			return XmlCustomFormatter.ToXmlName (value);
		}

		protected static string ToXmlNCName (string value)
		{
			return XmlCustomFormatter.ToXmlNCName (value);
		}

		protected static string ToXmlNmToken (string value)
		{
			return XmlCustomFormatter.ToXmlNmToken (value);
		}

		protected static string ToXmlNmTokens (string value)
		{
			return XmlCustomFormatter.ToXmlNmTokens (value);
		}

		protected XmlQualifiedName ToXmlQualifiedName (string value)
		{
			string name;
			string ns;
			int lastColon = value.LastIndexOf (':');
			string decodedValue = XmlConvert.DecodeName (value);
			if (lastColon < 0) {
				name = reader.NameTable.Add (decodedValue);
				ns = reader.LookupNamespace (String.Empty);
			} else {
				string prefix = value.Substring (0, lastColon);
				ns = reader.LookupNamespace (prefix);
				if (ns == null)
					throw new InvalidOperationException ("namespace " + prefix + "not defined");

				name = reader.NameTable.Add (value.Substring (lastColon + 1));
			}

			return new XmlQualifiedName (name, ns);
		}

		[MonoTODO ("Implement")]
		protected void UnknownAttribute (object o, XmlAttribute attr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void UnknownElement (object o, XmlElement elem)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void UnknownNode (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void UnreferencedObject (string id, object o)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

		protected class CollectionFixup {
			
			#region Fields

			XmlSerializationCollectionFixupCallback callback;
			object collection;
			object collectionItems;

			#endregion // Fields

			#region Constructors

			[MonoTODO]
			public CollectionFixup (object collection, XmlSerializationCollectionFixupCallback callback, object collectionItems)
			{
				this.callback = callback;
				this.collection = collection;
				this.collectionItems = collectionItems;
			}

			#endregion // Constructors

			#region Properties

			public XmlSerializationCollectionFixupCallback Callback { 
				get { return callback; }
			}

			public object Collection {
				get { return collection; }
			}

			public object CollectionItems {
				get { return collectionItems; }
			}

			#endregion // Properties
		}

		protected class Fixup {

			#region Fields

			object source;
			string[] ids;
			XmlSerializationFixupCallback callback;

			#endregion // Fields

			#region Constructors

			[MonoTODO]
			public Fixup (object o, XmlSerializationFixupCallback callback, int count) 
			{
				this.callback = callback;
			}

			[MonoTODO]
			public Fixup (object o, XmlSerializationFixupCallback callback, string[] ids)
			{
				this.callback = callback;
			}

			#endregion // Constructors

			#region Properties

			public XmlSerializationFixupCallback Callback {
				get { return callback; }
			}

			public string[] Ids {
				get { return ids; }
			}

			public object Source {
				get { return source; }
				set { source = value; }
			}

			#endregion // Properties
		}
	}
}

