//
// System.Xml.Serialization.XmlSerializationReader.cs
//
// Authors:
// 	Tim Coleman (tim@timcoleman.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;

namespace System.Xml.Serialization {
	public abstract class XmlSerializationReader {

		#region Fields

		XmlDocument document;
		XmlReader reader;
		ArrayList fixups;
		Hashtable collFixups;
		ArrayList collItemFixups;
		Hashtable typesCallbacks;
		ArrayList noIDTargets;
		Hashtable targets;
		Hashtable delayedListFixups;
		XmlSerializer eventSource;

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
		XmlQualifiedName arrayQName;
		#endregion

		internal void Initialize (XmlReader reader, XmlSerializer eventSource)
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
			this.reader = reader;
			this.eventSource = eventSource;
			arrayQName = new XmlQualifiedName ("Array", soapNS);
			InitIDs ();
		}
			
		internal virtual object ReadObject ()
		{
			throw new NotImplementedException ();
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
			collFixups = EnsureHashtable (collFixups);
			collFixups [fixup.Id] = fixup;

			if (delayedListFixups != null && delayedListFixups.ContainsKey (fixup.Id))
				fixup.CollectionItems = delayedListFixups [fixup.Id];
		}

		protected void AddFixup (Fixup fixup)
		{
			fixups = EnsureArrayList (fixups);
			fixups.Add (fixup);
		}

		void AddFixup (CollectionItemFixup fixup)
		{
			collItemFixups = EnsureArrayList (collItemFixups);
			collItemFixups.Add(fixup);
		}

		protected void AddReadCallback (string name, string ns, Type type, XmlSerializationReadCallback read)
		{
			WriteCallbackInfo info = new WriteCallbackInfo ();
			info.Type = type;
			info.TypeName = name;
			info.TypeNs = ns;
			info.Callback = read;
			typesCallbacks = EnsureHashtable (typesCallbacks);
			typesCallbacks.Add (new XmlQualifiedName (name, ns), info);
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
			if (na == string.Empty) {
				na = reader.GetAttribute (nil, w3InstanceNS);
				if (na == string.Empty) {
					na = reader.GetAttribute (nullX, w3InstanceNS2000);
					if (na == string.Empty)
						na = reader.GetAttribute (nullX, w3InstanceNS1999);
				}
			}
			return (na != string.Empty);
		}

		protected object GetTarget (string id)
		{
			if (targets == null) return null;
			return targets [id];
		}

		bool TargetReady (string id)
		{
			if (targets == null) return false;
			return targets.ContainsKey (id);
		}

		[MonoTODO ("Implement")]
		protected XmlQualifiedName GetXsiType ()
		{
			string typeName = Reader.GetAttribute ("xsi:type");
			if (typeName == string.Empty || typeName == null) return null;
			int i = typeName.IndexOf (":");
			if (i == -1) return new XmlQualifiedName (typeName, "");
			else 
			{
				string prefix = typeName.Substring(0,i);
				string name = typeName.Substring (i+1);
				return new XmlQualifiedName (name, Reader.LookupNamespace (prefix));
			}
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
			return ReadReferencedElement (Reader.LocalName, Reader.NamespaceURI);
		}

		WriteCallbackInfo GetCallbackInfo (XmlQualifiedName qname)
		{
			if (typesCallbacks == null) 
			{
				typesCallbacks = new Hashtable ();
				InitCallbacks ();
			}
			return (WriteCallbackInfo) typesCallbacks[qname];
		}

		protected object ReadReferencedElement (string name, string ns)
		{
			XmlQualifiedName qname = GetXsiType ();
			if (qname == null) qname = new XmlQualifiedName (name, ns);

			string id = Reader.GetAttribute ("id");
			object ob;

			if (qname == arrayQName)
			{
				CollectionFixup fixup = (collFixups != null) ? (CollectionFixup) collFixups[id] : null;
				if (ReadList (out ob))
				{
					// List complete (does not contain references)
					if (fixup != null)
					{
						fixup.Callback (fixup.Collection, ob);
						collFixups.Remove (id);
						ob = fixup.Collection;
					}
				}
				else if (fixup != null) 
				{
					fixup.CollectionItems = (object[])ob;
					ob = fixup.Collection;
				}
			}
			else
			{
				WriteCallbackInfo info = GetCallbackInfo (qname);
				if (info == null) throw CreateUnknownTypeException (qname);
				ob = info.Callback ();
			}
			AddTarget (id, ob);
			return ob;
		}

		bool ReadList (out object resultList)
		{
			string arrayType = Reader.GetAttribute ("arrayType", soapNS);
			XmlQualifiedName qn = ToXmlQualifiedName (arrayType);
			int i = qn.Name.LastIndexOf ('[');
			string dim = qn.Name.Substring (i);
			string itemType = qn.Name.Substring (0,i);
			int count = Int32.Parse (dim.Substring (1, dim.Length - 2));

			Array list;

			i = itemType.IndexOf ('['); if (i == -1) i = itemType.Length;
			string baseType = itemType.Substring (0,i);
			string arrayTypeName;

			if (qn.Namespace == XmlSchema.Namespace)
				arrayTypeName = TypeTranslator.GetPrimitiveTypeData (baseType).Type.FullName + itemType.Substring (i);
			else
			{
				WriteCallbackInfo info = GetCallbackInfo (new XmlQualifiedName (baseType,qn.Namespace));
				arrayTypeName = info.Type.FullName + itemType.Substring (i) + ", " + info.Type.Assembly.FullName;
			}

			list = Array.CreateInstance (Type.GetType (arrayTypeName), count);

			bool listComplete = true;

			Reader.ReadStartElement ();
			for (int n=0; n<count; n++)
			{
				Reader.MoveToContent ();
				string refid = Reader.GetAttribute ("href");
				string id;
				object item = ReadReferencingElement (itemType, qn.Namespace, out id);
				if (id == null) 
					list.SetValue (item,n);
				else
				{
					AddFixup (new CollectionItemFixup (list, n, id));
					listComplete = false;
				}
			}
			Reader.ReadEndElement ();

			resultList = list;
			return listComplete;
		}
		
		protected void ReadReferencedElements ()
		{
			reader.MoveToContent();
			XmlNodeType nt = reader.NodeType;
			while (nt != XmlNodeType.EndElement && nt != XmlNodeType.None) {
				ReadReferencedElement ();
				reader.MoveToContent ();
				nt = reader.NodeType;
			}

			// Fix arrays

			if (collItemFixups != null)
			{
				foreach (CollectionItemFixup itemFixup in collItemFixups)
					itemFixup.Collection.SetValue (GetTarget (itemFixup.Id), itemFixup.Index);
			}

			// Fills collections

			if (collFixups != null)
			{
				ICollection cfixups = collFixups.Values;
				foreach (CollectionFixup fixup in cfixups)
					fixup.Callback (fixup.Collection, fixup.CollectionItems);
			}

			// Fills class instances

			if (fixups != null)
			{
				foreach (Fixup fixup in fixups)
					fixup.Callback (fixup);
			}
		}

		protected object ReadReferencingElement (out string fixupReference)
		{
			return ReadReferencingElement (Reader.LocalName, Reader.NamespaceURI, false, out fixupReference);
		}

		protected object ReadReferencingElement (string name, string ns, out string fixupReference)
		{
			return ReadReferencingElement (name, ns, false, out fixupReference);
		}

		protected object ReadReferencingElement (string name,
							 string ns,
							 bool elementCanBeType,
							 out string fixupReference)
		{
			if (ReadNull ())
			{
				fixupReference = null;
				return null;
			}

			string refid = Reader.GetAttribute ("href");

			if (refid == string.Empty || refid == null)
			{
				fixupReference = null;

				XmlQualifiedName qname = GetXsiType ();
				if (qname == null) qname = new XmlQualifiedName (name, ns);

				if (qname == arrayQName)
				{
					delayedListFixups = EnsureHashtable (delayedListFixups);
					fixupReference = "__<" + delayedListFixups.Count + ">";
					object items;
					ReadList (out items);
					delayedListFixups [fixupReference] = items;
					return null;
				}
				else
				{
					WriteCallbackInfo info = GetCallbackInfo (qname);
					if (info == null)
						return ReadTypedPrimitive (qname);
					else
						return info.Callback();
				}
			}
			else
			{
				if (refid.StartsWith ("#")) refid = refid.Substring (1);

				Reader.ReadStartElement ();
				if (TargetReady (refid))
				{
					fixupReference = null;
					return GetTarget (refid);
				}
				else
				{
					fixupReference = refid;
					return null;
				}
			}
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

		protected object ReadTypedPrimitive (XmlQualifiedName qname)
		{
			if (qname == null) qname = GetXsiType ();
			TypeData typeData = TypeTranslator.GetPrimitiveTypeData (qname.Name);
			if (typeData == null || typeData.SchemaType != SchemaTypes.Primitive) throw new InvalidOperationException ("Unknown type: " + qname.Name);
			return XmlCustomFormatter.FromXmlString (typeData.Type, Reader.ReadElementString ());
		}

		protected XmlNode ReadXmlNode (bool wrapped)
		{
			XmlNode node = Document.ReadNode (reader);
			if (wrapped)
				return node.FirstChild;
			else
				return node;
		}

		[MonoTODO ("Implement")]
		protected void Referenced (object o)
		{
		}

		protected Array ShrinkArray (Array a, int length, Type elementType, bool isNullable)
		{
			if (length == 0 && isNullable) return null;
			if (a == null) return Array.CreateInstance (elementType, length);
			if (a.Length == length) return a;

			Array result = Array.CreateInstance (elementType, length);
			Array.Copy (a, result, length);
			return result;
		}

		protected byte[] ToByteArrayBase64 (bool isNull)
		{
			return Convert.FromBase64String (Reader.ReadString());
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

		protected void UnknownAttribute (object o, XmlAttribute attr)
		{
			int line_number, line_position;
			
			if (Reader is XmlTextReader){
				line_number = ((XmlTextReader)Reader).LineNumber;
				line_position = ((XmlTextReader)Reader).LinePosition;
			} else {
				line_number = 0;
				line_position = 0;
			}
			
			eventSource.OnUnknownAttribute (new XmlAttributeEventArgs (attr, line_number, line_position, o));
		}

		protected void UnknownElement (object o, XmlElement elem)
		{
			int line_number, line_position;
			
			if (Reader is XmlTextReader){
				line_number = ((XmlTextReader)Reader).LineNumber;
				line_position = ((XmlTextReader)Reader).LinePosition;
			} else {
				line_number = 0;
				line_position = 0;
			}
			
			eventSource.OnUnknownElement (new XmlElementEventArgs (elem, line_number, line_position,o));
		}

		protected void UnknownNode (object o)
		{
			int line_number, line_position;
			
			if (Reader is XmlTextReader){
				line_number = ((XmlTextReader)Reader).LineNumber;
				line_position = ((XmlTextReader)Reader).LinePosition;
			} else {
				line_number = 0;
				line_position = 0;
			}
			
			eventSource.OnUnknownNode (new XmlNodeEventArgs(line_number, line_position, Reader.LocalName, Reader.Name, Reader.NamespaceURI, Reader.NodeType, o, Reader.Value));
			if (Reader.NodeType == XmlNodeType.Attribute)
			{
				XmlAttribute att = (XmlAttribute) ReadXmlNode (false);
				UnknownAttribute (o, att);
				return;
			}
			else if (Reader.NodeType == XmlNodeType.Element)
			{
				XmlElement elem = (XmlElement) ReadXmlNode (false);
				UnknownElement (o, elem);
				return;
			}
			else
			{
				Reader.Skip();
				if (Reader.ReadState == ReadState.EndOfFile) 
					throw new InvalidOperationException ("End of document found");
			}
		}

		protected void UnreferencedObject (string id, object o)
		{
			eventSource.OnUnreferencedObject (new UnreferencedObjectEventArgs (o,id));
		}

		#endregion // Methods

		class WriteCallbackInfo
		{
			public Type Type;
			public string TypeName;
			public string TypeNs;
			public XmlSerializationReadCallback Callback;
		}

		protected class CollectionFixup {
			
			XmlSerializationCollectionFixupCallback callback;
			object collection;
			object collectionItems;
			string id;

			public CollectionFixup (object collection, XmlSerializationCollectionFixupCallback callback, string id)
			{
				this.callback = callback;
				this.collection = collection;
				this.id = id;
			}

			public XmlSerializationCollectionFixupCallback Callback { 
				get { return callback; }
			}

			public object Collection {
				get { return collection; }
			}

			public object Id {
				get { return id; }
			}

			internal object CollectionItems
			{
				get { return collectionItems; }
				set { collectionItems = value; }
			}
		}

		protected class Fixup {

			object source;
			string[] ids;
			XmlSerializationFixupCallback callback;

			public Fixup (object o, XmlSerializationFixupCallback callback, int count) 
			{
				this.source = o;
				this.callback = callback;
				this.ids = new string[count];
			}

			public Fixup (object o, XmlSerializationFixupCallback callback, string[] ids)
			{
				this.source = o;
				this.ids = ids;
				this.callback = callback;
			}

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
		}

		protected class CollectionItemFixup 
		{
			Array list;
			int index;
			string id;

			public CollectionItemFixup (Array list, int index, string id)
			{
				this.list = list;
				this.index = index;
				this.id = id;
			}

			public Array Collection
			{
				get { return list; }
			}

			public int Index
			{
				get { return index; }
			}

			public string Id
			{
				get { return id; }
			}
		}

	}
}

