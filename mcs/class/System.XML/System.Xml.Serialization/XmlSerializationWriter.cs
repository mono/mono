//
// System.Xml.Serialization.XmlSerializationWriter.cs
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
	public abstract class XmlSerializationWriter {

		#region Fields

		ArrayList namespaces;
		XmlWriter writer;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected XmlSerializationWriter ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		protected ArrayList Namespaces {
			get { return namespaces; }
			set { namespaces = value; }
		}

		protected XmlWriter Writer {
			get { return writer; }
			set { writer = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO ("Implement")]
		protected void AddWriteCallback (Type type, string typeName, string typeNs, XmlSerializationWriteCallback callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateMismatchChoiceException (string value, string elementName, string enumValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateUnknownAnyElementException (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateUnknownTypeException (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected Exception CreateUnknownTypeException (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static byte[] FromByteArrayBase64 (byte[] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromByteArrayHex (byte[] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromChar (char value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromDate (DateTime value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromDateTime (DateTime value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromEnum (long value, string[] values, long[] ids)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromTime (DateTime value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromXmlName (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromXmlNCName (string ncName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromXmlNmToken (string nmToken)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected static string FromXmlNmTokens (string nmTokens)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected string FromXmlQualifiedName (XmlQualifiedName xmlQualifiedName)
		{
			throw new NotImplementedException ();
		}


		protected abstract void InitCallbacks ();

		[MonoTODO ("Implement")]
		protected void TopLevelElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteAttribute (string localName, byte[] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteAttribute (string localName, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteAttribute (string localName, string ns, byte[] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteAttribute (string localName, string ns, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteAttribute (string prefix, string localName, string ns, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementEncoded (XmlNode node, string name, string ns, bool isNullable, bool any)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementLiteral (XmlNode node, string name, string ns, bool isNullable, bool any)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementQualifiedName (string localName, XmlQualifiedName value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementQualifiedName (string localName, string ns, XmlQualifiedName value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementQualifiedName (string localName, XmlQualifiedName value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementQualifiedName (string localName, string ns, XmlQualifiedName value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementString (string localName, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementString (string localName, string ns, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementString (string localName, string value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementString (string localName, string ns, string value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, byte[] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, byte[] value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, string ns, byte[] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, string ns, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, string value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, string ns, byte[] value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, string ns, string value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteEmptyTag (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteEmptyTag (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteEndElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteEndElement (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteId (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullableQualifiedNameEncoded (string name, string ns, XmlQualifiedName value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullableQualifiedNameLiteral (string name, string ns, XmlQualifiedName value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullableStringEncoded (string name, string ns, string value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullableStringEncodedRaw (string name, string ns, byte[] value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullableStringEncodedRaw (string name, string ns, string value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullableStringLiteral (string name, string ns, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullableStringLiteralRaw (string name, string ns, byte[] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullableStringLiteralRaw (string name, string ns, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullTagEncoded (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullTagEncoded (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullTagLiteral (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteNullTagLiteral (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WritePotentiallyReferencingElement (string n, string ns, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WritePotentiallyReferencingElement (string n, string ns, object o, Type ambientType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WritePotentiallyReferencingElement (string n, string ns, object o, Type ambientType, bool suppressReference)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WritePotentiallyReferencingElement (string n, string ns, object o, Type ambientType, bool suppressReference, bool isNullable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteReferencedElements ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteReferencingElement (string n, string ns, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteReferencingElement (string n, string ns, object o, bool isNullable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteSerializable (IXmlSerializable serializable, string name, string ns, bool isNullable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteStartDocument ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteStartElement (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteStartElement (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteStartElement (string name, string ns, bool writePrefixed)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteStartElement (string name, string ns, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteStartElement (string name, string ns, object o, bool writePrefixed)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteTypedPrimitive (string name, string ns, object o, bool xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteValue (byte[] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteValue (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteXmlAttribute (XmlNode node)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteXmlAttribute (XmlNode node, object container)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteXsiType (string name, string ns)
		{
			throw new NotImplementedException ();
		}


		#endregion
	}
}
