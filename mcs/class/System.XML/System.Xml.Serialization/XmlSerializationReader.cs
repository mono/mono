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
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;

namespace System.Xml.Serialization 
{
#if NET_2_0
	[MonoTODO]
	// FIXME: provide expected elements/attributes on unknown elements/attributs
#endif
	public abstract class XmlSerializationReader 
#if NET_2_0
		: XmlSerializationGeneratedCode
#endif
	{

		#region Fields

#if !MOONLIGHT
		XmlDocument document;
#endif
		XmlReader reader;
		ArrayList fixups;
		Hashtable collFixups;
		ArrayList collItemFixups;
		Hashtable typesCallbacks;
		ArrayList noIDTargets;
		Hashtable targets;
		Hashtable delayedListFixups;
		XmlSerializer eventSource;
		int delayedFixupId = 0;
		Hashtable referencedObjects;
		int readCount, whileIterationCount;

		string w3SchemaNS;
		string w3InstanceNS;
		string w3InstanceNS2000;
		string w3InstanceNS1999;
		string soapNS;
		string wsdlNS;
		string nullX;
		string nil;
		string typeX;
		string arrayType;
		XmlQualifiedName arrayQName;
		#endregion

		internal void Initialize (XmlReader reader, XmlSerializer eventSource)
		{
			w3SchemaNS = reader.NameTable.Add (XmlSchema.Namespace);
			w3InstanceNS = reader.NameTable.Add (XmlSchema.InstanceNamespace);
			w3InstanceNS2000 = reader.NameTable.Add ("http://www.w3.org/2000/10/XMLSchema-instance");
			w3InstanceNS1999 = reader.NameTable.Add ("http://www.w3.org/1999/XMLSchema-instance");
			soapNS = reader.NameTable.Add (XmlSerializer.EncodingNamespace);
			wsdlNS = reader.NameTable.Add (XmlSerializer.WsdlNamespace);
			nullX = reader.NameTable.Add ("null");
			nil = reader.NameTable.Add ("nil");
			typeX = reader.NameTable.Add ("type");
			arrayType = reader.NameTable.Add ("arrayType");
			this.reader = reader;
			this.eventSource = eventSource;
			arrayQName = new XmlQualifiedName ("Array", soapNS);
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

#if !MOONLIGHT
		protected XmlDocument Document
		{
			get {
				if (document == null)
					document = new XmlDocument (reader.NameTable);

				return document;
			}
		}
#endif

		protected XmlReader Reader {
			get { return reader; }
		}

		[MonoTODO]
		protected bool IsReturnValue
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}

		}

#if NET_2_0
		protected int ReaderCount {
			get { return readCount; }
		}
#endif

		#region Methods

		protected void AddFixup (CollectionFixup fixup)
		{
			collFixups = EnsureHashtable (collFixups);
			collFixups [fixup.Id] = fixup;

			if (delayedListFixups != null && delayedListFixups.ContainsKey (fixup.Id)) {
				fixup.CollectionItems = delayedListFixups [fixup.Id];
				delayedListFixups.Remove (fixup.Id);
			}
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
			case XmlNodeType.Element:
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

		protected Exception CreateCtorHasSecurityException (string typeName)
		{
			string message = string.Format ("The type '{0}' cannot"
				+ " be serialized because its parameterless"
				+ " constructor is decorated with declarative"
				+ " security permission attributes."
				+ " Consider using imperative asserts or demands"
				+ " in the constructor.", typeName);
			return new InvalidOperationException (message);
		}

		protected Exception CreateInaccessibleConstructorException (string typeName)
		{
			string message = string.Format ("{0} cannot be serialized"
				+ " because it does not have a default public"
				+ " constructor.", typeName);
			return new InvalidOperationException (message);
		}

		protected Exception CreateAbstractTypeException (string name, string ns)
		{
			string message = "The specified type is abstrace: name='" + name + "' namespace='" + ns + "', at " + CurrentTag ();
			return new InvalidOperationException (message);
		}

		protected Exception CreateInvalidCastException (Type type, object value)
		{
			string message = String.Format (CultureInfo.InvariantCulture, "Cannot assign object of type {0} to an object of " +
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
			string message = CurrentTag () + " was not expected";
			return new InvalidOperationException (message);
		}

		protected Exception CreateUnknownTypeException (XmlQualifiedName type)
		{
			string message = "The specified type was not recognized: name='" + type.Name + "' namespace='" + type.Namespace + "', at " + CurrentTag ();
			return new InvalidOperationException (message);
		}

#if NET_2_0
		protected void CheckReaderCount (ref int whileIterations, ref int readerCount)
		{
			whileIterations = whileIterationCount;
			readerCount = readCount;
		}
#endif

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

		[MonoTODO]
		protected void FixupArrayRefs (object fixup)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		protected object GetTarget (string id)
		{
			if (targets == null) return null;
			object ob = targets [id];
			if (ob != null) {
				if (referencedObjects == null) referencedObjects = new Hashtable ();
				referencedObjects [ob] = ob;
			}
			return ob;
		}

		bool TargetReady (string id)
		{
			if (targets == null) return false;
			return targets.ContainsKey (id);
		}

		protected XmlQualifiedName GetXsiType ()
		{
			string typeName = Reader.GetAttribute (typeX, XmlSchema.InstanceNamespace);
			
			if (typeName == string.Empty || typeName == null) {
				typeName = Reader.GetAttribute (typeX, w3InstanceNS1999);
				if (typeName == string.Empty || typeName == null) {
					typeName = Reader.GetAttribute (typeX, w3InstanceNS2000);
					if (typeName == string.Empty || typeName == null)
						return null;
				}
			}
			
			int i = typeName.IndexOf (":");
			if (i == -1) return new XmlQualifiedName (typeName, Reader.NamespaceURI);
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

#if !MOONLIGHT
		protected void ParseWsdlArrayType (XmlAttribute attr)
		{
			if (attr.NamespaceURI == wsdlNS && attr.LocalName == arrayType)
			{
				string ns = "", type, dimensions;
				TypeTranslator.ParseArrayType (attr.Value, out type, out ns, out dimensions);
				if (ns != "") ns = Reader.LookupNamespace (ns) + ":";
				attr.Value = ns + type + dimensions;
			}
		}
#endif

		protected XmlQualifiedName ReadElementQualifiedName ()
		{
			readCount++;

			if (reader.IsEmptyElement) {
				reader.Skip();
				return ToXmlQualifiedName (String.Empty);
			}

			reader.ReadStartElement ();
			XmlQualifiedName xqn = ToXmlQualifiedName(reader.ReadString ());
			reader.ReadEndElement ();
			return xqn;
		}

		protected void ReadEndElement ()
		{
			readCount++;

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

			readCount++;

			if (reader.IsEmptyElement) {
				reader.Skip();
				return true;
			}

			reader.ReadStartElement();
#if MOONLIGHT
			while (reader.NodeType != XmlNodeType.EndElement)
				reader.Skip ();
#else
			while (reader.NodeType != XmlNodeType.EndElement)
				UnknownNode (null);
#endif

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

			readCount++;
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
			readCount++;
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

			// it takes precedence over xsi:type.
			// Sometimes there are array types in WSDL,
			// which are not reflected in client proxies.
			// In SOAP messages, they are marked
			// soap-env:arrayType, so use it (this could coexist
			// with xsi:type, which indicates the type in WSDL).
			// See bug #79057.
			string arrayTypeVal = Reader.GetAttribute (arrayType, soapNS);

			if (qname == arrayQName || arrayTypeVal != null && arrayTypeVal.Length > 0)
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
				if (info == null)
					ob = ReadTypedPrimitive (qname, id != null);
				else
					ob = info.Callback();
			}
			AddTarget (id, ob);
			return ob;
		}
		
		bool ReadList (out object resultList)
		{
			string arrayTypeAttr = Reader.GetAttribute (arrayType, soapNS);
			if (arrayTypeAttr == null) arrayTypeAttr = Reader.GetAttribute (arrayType, wsdlNS);
			
			XmlQualifiedName qn = ToXmlQualifiedName (arrayTypeAttr);
			int i = qn.Name.LastIndexOf ('[');
			string dim = qn.Name.Substring (i);
			string itemType = qn.Name.Substring (0,i);
			int count = Int32.Parse (dim.Substring (1, dim.Length - 2), CultureInfo.InvariantCulture);

			Array list;

			i = itemType.IndexOf ('['); if (i == -1) i = itemType.Length;
			string baseType = itemType.Substring (0,i);
			string arrayTypeName;

			if (qn.Namespace == w3SchemaNS)
				arrayTypeName = TypeTranslator.GetPrimitiveTypeData (baseType).Type.FullName + itemType.Substring (i);
			else
			{
				WriteCallbackInfo info = GetCallbackInfo (new XmlQualifiedName (baseType,qn.Namespace));
				arrayTypeName = info.Type.FullName + itemType.Substring (i) + ", " + info.Type.Assembly.FullName;
			}

			list = Array.CreateInstance (Type.GetType (arrayTypeName), count);

			bool listComplete = true;

			if (Reader.IsEmptyElement) {
				readCount++;
				Reader.Skip ();
			} else {
				Reader.ReadStartElement ();
				for (int n=0; n<count; n++)
				{
					whileIterationCount++;
					readCount++;
					Reader.MoveToContent ();
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
				whileIterationCount = 0;
				Reader.ReadEndElement ();
			}

			resultList = list;
			return listComplete;
		}
		
		protected void ReadReferencedElements ()
		{
			reader.MoveToContent();
			XmlNodeType nt = reader.NodeType;
			while (nt != XmlNodeType.EndElement && nt != XmlNodeType.None) {
				whileIterationCount++;
				readCount++;
				ReadReferencedElement ();
				reader.MoveToContent ();
				nt = reader.NodeType;
			}
			whileIterationCount = 0;

			// Registers delayed list
			
			if (delayedListFixups != null)
			{
				foreach (DictionaryEntry entry in delayedListFixups)
					AddTarget ((string)entry.Key, entry.Value);
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
			
			if (targets != null) {
				foreach (DictionaryEntry e in targets) {
					if (e.Value != null && (referencedObjects == null || !referencedObjects.Contains (e.Value)))
						UnreferencedObject ((string)e.Key, e.Value);
				}
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
				string arrayTypeAttr = Reader.GetAttribute (arrayType, soapNS);

				if (qname == arrayQName || arrayTypeAttr != null)
				{
					delayedListFixups = EnsureHashtable (delayedListFixups);
					fixupReference = "__<" + (delayedFixupId++) + ">";
					object items;
					ReadList (out items);
					delayedListFixups [fixupReference] = items;
					return null;
				}
				else
				{
					WriteCallbackInfo info = GetCallbackInfo (qname);
					if (info == null)
						return ReadTypedPrimitive (qname, true);
					else
						return info.Callback();
				}
			}
			else
			{
				if (refid.StartsWith ("#")) refid = refid.Substring (1);

				readCount++;
				Reader.Skip ();
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
			if (ReadNull ()) return null;
			int depth = reader.Depth;
			readCount++;
			serializable.ReadXml (reader);
			Reader.MoveToContent ();
			while (reader.Depth > depth)
				reader.Skip ();
			if (reader.Depth == depth && reader.NodeType == XmlNodeType.EndElement)
				reader.ReadEndElement ();
			return serializable;
		}

		protected string ReadString (string value)
		{
			readCount++;
			if (value == null || value == String.Empty)
				return reader.ReadString ();

			return (value + reader.ReadString ());
		}

		protected object ReadTypedPrimitive (XmlQualifiedName qname)
		{
			return ReadTypedPrimitive (qname, false);
		}
		
		object ReadTypedPrimitive (XmlQualifiedName qname, bool reportUnknown)
		{
			if (qname == null) qname = GetXsiType ();
			
			TypeData typeData = TypeTranslator.FindPrimitiveTypeData (qname.Name);
			if (typeData == null || typeData.SchemaType != SchemaTypes.Primitive)
			{
#if MOONLIGHT
				// skip everything
				reader.Skip ();
				return new Object ();
#else
				// Put everything into a node array
				readCount++;
				XmlNode node = Document.ReadNode (reader);
				
				if (reportUnknown)
					OnUnknownNode (node, null, null);

				if (node.ChildNodes.Count == 0 && node.Attributes.Count == 0)
					return new Object ();

				XmlElement elem = node as XmlElement;
				
				if (elem == null)
					return new XmlNode[] {node};
				else {
					XmlNode[] nodes = new XmlNode[elem.Attributes.Count + elem.ChildNodes.Count];
					int n = 0;
					foreach (XmlNode no in elem.Attributes)
						nodes[n++] = no;
					foreach (XmlNode no in elem.ChildNodes)
						nodes[n++] = no;
					return nodes;
				}
#endif
			}

			if (typeData.Type == typeof (XmlQualifiedName)) return ReadNullableQualifiedName ();
			readCount++;
			return XmlCustomFormatter.FromXmlString (typeData, Reader.ReadElementString ());
		}

#if !MOONLIGHT
		protected XmlNode ReadXmlNode (bool wrapped)
		{
			readCount++;
			XmlNode node = Document.ReadNode (reader);
			if (wrapped)
				return node.FirstChild;
			else
				return node;
		}

		protected XmlDocument ReadXmlDocument (bool wrapped)
		{
			readCount++;

			if (wrapped)
				reader.ReadStartElement ();
			reader.MoveToContent ();
			XmlDocument doc = new XmlDocument ();
			XmlNode node = doc.ReadNode (reader);
			doc.AppendChild (node);
			
			if (wrapped)
				reader.ReadEndElement ();
				
			return doc;
		}
#endif

		protected void Referenced (object o)
		{
			if (o != null) {
				if (referencedObjects == null) referencedObjects = new Hashtable ();
				referencedObjects [o] = o;
			}
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
			readCount++;
			if (isNull) {
				Reader.ReadString ();
				return null;
			}
			else
				return ToByteArrayBase64 (Reader.ReadString ());
		}

		protected static byte[] ToByteArrayBase64 (string value)
		{
			return Convert.FromBase64String (value);
		}

		protected byte[] ToByteArrayHex (bool isNull)
		{
			readCount++;
			if (isNull) {
				Reader.ReadString ();
				return null;
			}
			else
				return ToByteArrayHex (Reader.ReadString ());
		}

		protected static byte[] ToByteArrayHex (string value)
		{
			return XmlConvert.FromBinHexString (value);
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
					throw new InvalidOperationException ("namespace " + prefix + " not defined");

				name = reader.NameTable.Add (value.Substring (lastColon + 1));
			}

			return new XmlQualifiedName (name, ns);
		}

#if MOONLIGHT
		protected void UnknownNode (object o)
		{
			throw new NotSupportedException ();
		}
#else
		protected void UnknownAttribute (object o, XmlAttribute attr)
		{
			UnknownAttribute (o, attr, null);
		}

#if NET_2_0
		protected
#endif
		void UnknownAttribute (object o, XmlAttribute attr, string qnames)
		{
			int line_number, line_position;
			
			if (Reader is XmlTextReader){
				line_number = ((XmlTextReader)Reader).LineNumber;
				line_position = ((XmlTextReader)Reader).LinePosition;
			} else {
				line_number = 0;
				line_position = 0;
			}

			XmlAttributeEventArgs args = new XmlAttributeEventArgs (attr, line_number, line_position, o);
#if NET_2_0
			args.ExpectedAttributes = qnames;
#endif

			if (eventSource != null)
				eventSource.OnUnknownAttribute (args);
		}

		protected void UnknownElement (object o, XmlElement elem)
		{
			UnknownElement (o, elem, null);
		}

#if NET_2_0
		protected
#endif
		void UnknownElement (object o, XmlElement elem, string qnames)
		{
			int line_number, line_position;
			
			if (Reader is XmlTextReader){
				line_number = ((XmlTextReader)Reader).LineNumber;
				line_position = ((XmlTextReader)Reader).LinePosition;
			} else {
				line_number = 0;
				line_position = 0;
			}

			XmlElementEventArgs args = new XmlElementEventArgs (elem, line_number, line_position, o);
#if NET_2_0
			args.ExpectedElements = qnames;
#endif

			if (eventSource != null)
				eventSource.OnUnknownElement (args);
		}

		protected void UnknownNode (object o)
		{
			UnknownNode (o, null);
		}

#if NET_2_0
		protected
#endif
		void UnknownNode (object o, string qnames)
		{
			OnUnknownNode (ReadXmlNode (false), o, qnames);
		}
		
		void OnUnknownNode (XmlNode node, object o, string qnames)
		{
			int line_number, line_position;
			
			if (Reader is XmlTextReader){
				line_number = ((XmlTextReader)Reader).LineNumber;
				line_position = ((XmlTextReader)Reader).LinePosition;
			} else {
				line_number = 0;
				line_position = 0;
			}
	
			if (node is XmlAttribute)
			{
				UnknownAttribute (o, (XmlAttribute)node, qnames);
				return;
			}
			else if (node is XmlElement)
			{
				UnknownElement (o, (XmlElement) node, qnames);
				return;
			}
			else
			{
				if (eventSource != null)
					eventSource.OnUnknownNode (new XmlNodeEventArgs(line_number, line_position, node.LocalName, node.Name, node.NamespaceURI, node.NodeType, o, node.Value));
	
				if (Reader.ReadState == ReadState.EndOfFile) 
					throw new InvalidOperationException ("End of document found");
			}
		}
#endif // !NET_2_1

		protected void UnreferencedObject (string id, object o)
		{
			if (eventSource != null)
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
		
#if NET_2_0
		[MonoTODO]
		protected bool DecodeName
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		protected string CollapseWhitespace (string value)
		{
			throw new NotImplementedException ();
		}
				
		[MonoTODO]
		protected Exception CreateBadDerivationException (
			string xsdDerived, 
			string nsDerived, 
			string xsdBase, 
			string nsBase, 
			string clrDerived, 
			string clrBase)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected Exception CreateInvalidCastException (Type type, object value, string id)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected Exception CreateMissingIXmlSerializableType (string name, string ns, string clrType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected string ReadString (string value, bool trim)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected object ReadTypedNull (XmlQualifiedName type)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected static Assembly ResolveDynamicAssembly (string assemblyFullName)
		{
			throw new NotImplementedException ();
		}

#endif

	}
}
