//
// System.Xml.Serialization.XmlSerializationReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.Xml;

namespace System.Xml.Serialization {
	public abstract class XmlSerializationReader {

		#region Fields

		XmlDocument document;
		XmlReader reader;

		#endregion

		[MonoTODO]
		protected XmlSerializationReader ()
		{
			throw new NotImplementedException ();
		}

		protected XmlDocument Document {
			get { return document; }
		}

		protected XmlReader Reader {
			get { return reader; }
		}

		#region Methods

		[MonoTODO ("Implement")]
		protected void AddFixup (XmlSerializationReader.CollectionFixup fixup)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void AddFixup (XmlSerializationReader.Fixup fixup)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void AddReadCallback (string name, string ns, Type type, XmlSerializationReadCallback read)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void AddTarget (string id, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateAbstractTypeException (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateInvalidCastException (string name, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateReadOnlyCollectionException (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateUnknownConstantException (string value, Type enumType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateUnknownNodeException ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateUnknownTypeException (XmlQualifiedName type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Array EnsureArrayIndex (Array a, int index, Type elementType)
		{
			throw new NotImplementedException ();
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

		[MonoTODO ("Implement")]
		protected bool GetNullAttr ()
		{
			throw new NotImplementedException ();
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

		[MonoTODO ("Implement")]
		protected bool IsXmlnsAttribute (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void ParseWsdlArrayType (XmlAttribute attr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected XmlQualifiedName ReadElementQualifiedName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void ReadEndElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected bool ReadNull ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected XmlQualifiedName ReadNullableQualifiedName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected string ReadNullableString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected bool ReadReference (out string fixupReference)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected object ReadReferencedElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected object ReadReferencedElement (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void ReadReferencedElements ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected object ReadReferencingElement (out string fixupReference)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected object ReadReferencingElement (string name, string ns, out string fixupReference)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected object ReadReferencingElement (string name, string ns, bool elementCanBeType, out string fixupReference)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected IXmlSerializable ReadSerializable (IXmlSerializable serializable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected string ReadString (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected object ReadTypedPrimitive (XmlQualifiedName type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected XmlNode ReadXmlNode (bool wrapped)
		{
			throw new NotImplementedException ();
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

		[MonoTODO ("Implement")]
		protected static char ToChar (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static DateTime ToDate (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static DateTime ToDateTime (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static long ToEnum (string value, Hashtable h, string typeName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static DateTime ToTime (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string ToXmlName (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string ToXmlNCName (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string ToXmlNmToken (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string ToXmlNmTokens (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected XmlQualifiedName ToXmlQualifiedName (string value)
		{
			throw new NotImplementedException ();
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

