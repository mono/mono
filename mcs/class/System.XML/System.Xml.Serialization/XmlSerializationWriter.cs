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
using System.Xml.Schema;

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

		protected Exception CreateMismatchChoiceException (string value, string elementName, string enumValue)
		{
			string message = String.Format ("Value of {0} mismatches the type of {1}, you need to set it to {2}.", elementName, value, enumValue);
			return new InvalidOperationException (message);
		}

		protected Exception CreateUnknownAnyElementException (string name, string ns)
		{
			string message = String.Format ("The XML element named '{0}' from namespace '{1}' was not expected. The XML element name and namespace must match those provided via XmlAnyElementAttribute(s).", name, ns);
			return new InvalidOperationException (message);
		}

		protected Exception CreateUnknownTypeException (object o)
		{
			return CreateUnknownTypeException (o.GetType ());
		}

		protected Exception CreateUnknownTypeException (Type type)
		{
			string message = String.Format ("The type {0} may not be used in this context.", type);
			return new InvalidOperationException (message);
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

		protected static string FromChar (char value)
		{
			return ((int) value).ToString ();
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
			return XmlCustomFormatter.FromXmlName (name);
		}

		[MonoTODO ("Implement")]
		protected static string FromXmlNCName (string ncName)
		{
			return XmlCustomFormatter.FromXmlNCName (ncName);
		}

		[MonoTODO ("Implement")]
		protected static string FromXmlNmToken (string nmToken)
		{
			return XmlCustomFormatter.FromXmlNmToken (nmToken);
		}

		[MonoTODO ("Implement")]
		protected static string FromXmlNmTokens (string nmTokens)
		{
			return XmlCustomFormatter.FromXmlNmTokens (nmTokens);
		}

		[MonoTODO ("Implement")]
		protected string FromXmlQualifiedName (XmlQualifiedName xmlQualifiedName)
		{
			return GetQualifiedName (xmlQualifiedName.Name, xmlQualifiedName.Namespace).ToString ();
		}

		private XmlQualifiedName GetQualifiedName (string name, string ns)
		{
			WriteAttribute ("xmlns", "q1", null, ns);
			return new XmlQualifiedName (name, "q1");
		}

		protected abstract void InitCallbacks ();

		[MonoTODO ("Implement")]
		protected void TopLevelElement ()
		{
			throw new NotImplementedException ();
		}

		protected void WriteAttribute (string localName, byte[] value)
		{
			WriteAttribute (localName, String.Empty, value);
		}

		protected void WriteAttribute (string localName, string value)
		{
			WriteAttribute (String.Empty, localName, String.Empty, value);
		}

		[MonoTODO ("Implement")]
		protected void WriteAttribute (string localName, string ns, byte[] value)
		{
			throw new NotImplementedException ();
		}

		protected void WriteAttribute (string localName, string ns, string value)
		{
			WriteAttribute (String.Empty, localName, ns, value);
		}

		protected void WriteAttribute (string prefix, string localName, string ns, string value)
		{
			Writer.WriteAttributeString (prefix, localName, ns, value);
		}

		[MonoTODO ("Implement")]
		protected void WriteElementEncoded (XmlNode node, string name, string ns, bool isNullable, bool any)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementLiteral (XmlNode node, string name, string ns, bool isNullable, bool any)
		{
			WriteStartElement (name, ns);
			node.WriteTo (Writer);
			WriteEndElement ();
		}

		protected void WriteElementQualifiedName (string localName, XmlQualifiedName value)
		{
			WriteElementQualifiedName (localName, String.Empty, value, null);
		}

		protected void WriteElementQualifiedName (string localName, string ns, XmlQualifiedName value)
		{
			WriteElementQualifiedName (localName, ns, value, null);
		}

		protected void WriteElementQualifiedName (string localName, XmlQualifiedName value, XmlQualifiedName xsiType)
		{
			WriteElementQualifiedName (localName, String.Empty, value, xsiType);
		}

		protected void WriteElementQualifiedName (string localName, string ns, XmlQualifiedName value, XmlQualifiedName xsiType)
		{
			WriteStartElement (localName, ns);
			Writer.WriteString (FromXmlQualifiedName (value));
			WriteEndElement ();
		}

		protected void WriteElementString (string localName, string value)
		{
			WriteElementString (localName, String.Empty, value, null);
		}

		protected void WriteElementString (string localName, string ns, string value)
		{
			WriteElementString (localName, ns, value, null);
		}

		protected void WriteElementString (string localName, string value, XmlQualifiedName xsiType)
		{
			WriteElementString (localName, String.Empty, value, xsiType);
		}

		[MonoTODO ("Implement")]
		protected void WriteElementString (string localName, string ns, string value, XmlQualifiedName xsiType)
		{
			WriteStartElement (localName, ns);

			if (xsiType != null)
				WriteXsiType (xsiType.Name, xsiType.Namespace);

			Writer.WriteString (value);
			WriteEndElement ();
		}

		protected void WriteElementStringRaw (string localName, byte[] value)
		{
			WriteElementStringRaw (localName, String.Empty, value, null);
		}

		protected void WriteElementStringRaw (string localName, string value)
		{
			WriteElementStringRaw (localName, String.Empty, value, null);
		}

		protected void WriteElementStringRaw (string localName, byte[] value, XmlQualifiedName xsiType)
		{
			WriteElementStringRaw (localName, String.Empty, value, xsiType);
		}

		protected void WriteElementStringRaw (string localName, string ns, byte[] value)
		{
			WriteElementStringRaw (localName, ns, value, null);
		}

		protected void WriteElementStringRaw (string localName, string ns, string value)
		{
			WriteElementStringRaw (localName, ns, value, null);
		}

		protected void WriteElementStringRaw (string localName, string value, XmlQualifiedName xsiType)
		{
			WriteElementStringRaw (localName, String.Empty, value, null);
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, string ns, byte[] value, XmlQualifiedName xsiType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		protected void WriteElementStringRaw (string localName, string ns, string value, XmlQualifiedName xsiType)
		{
			WriteStartElement (localName, ns);

			if (xsiType != null)
				WriteXsiType (xsiType.Name, xsiType.Namespace);

			Writer.WriteRaw (value);
			WriteEndElement ();
		}

		protected void WriteEmptyTag (string name)
		{
			WriteEmptyTag (name, String.Empty);
		}

		[MonoTODO ("Verify")]
		protected void WriteEmptyTag (string name, string ns)
		{
			Writer.WriteStartElement (name, ns);
			Writer.WriteEndElement ();
		}

		protected void WriteEndElement ()
		{
			WriteEndElement (null);
		}

		[MonoTODO ("Implement")]
		protected void WriteEndElement (object o)
		{
			Writer.WriteEndElement ();
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

		protected void WriteNullTagEncoded (string name)
		{
			WriteNullTagEncoded (name, String.Empty);
		}

		[MonoTODO ("Implement")]
		protected void WriteNullTagEncoded (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		protected void WriteNullTagLiteral (string name)
		{
			WriteNullTagLiteral (name, String.Empty);
		}

		[MonoTODO ("Implement")]
		protected void WriteNullTagLiteral (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		protected void WritePotentiallyReferencingElement (string n, string ns, object o)
		{
			WritePotentiallyReferencingElement (n, ns, o, null, false, false);
		}

		protected void WritePotentiallyReferencingElement (string n, string ns, object o, Type ambientType)
		{
			WritePotentiallyReferencingElement (n, ns, o, ambientType, false, false);
		}

		protected void WritePotentiallyReferencingElement (string n, string ns, object o, Type ambientType, bool suppressReference)
		{
			WritePotentiallyReferencingElement (n, ns, o, ambientType, suppressReference, false);
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

		protected void WriteReferencingElement (string n, string ns, object o)
		{
			WriteReferencingElement (n, ns, o, false);
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

		protected void WriteStartDocument ()
		{
			Writer.WriteStartDocument ();
		}

		protected void WriteStartElement (string name)
		{
			WriteStartElement (name, String.Empty, null, false);
		}

		protected void WriteStartElement (string name, string ns)
		{
			WriteStartElement (name, ns, null, false);
		}

		protected void WriteStartElement (string name, string ns, bool writePrefixed)
		{
			WriteStartElement (name, ns, null, writePrefixed);
		}

		protected void WriteStartElement (string name, string ns, object o)
		{
			WriteStartElement (name, ns, o, false);
		}

		[MonoTODO]
		protected void WriteStartElement (string name, string ns, object o, bool writePrefixed)
		{
			if (writePrefixed)
				Writer.WriteStartElement (String.Empty, name, ns);
			else
				Writer.WriteStartElement (name, ns);
		}

		[MonoTODO ("Include XMLSchema-Instance namespace")]
		protected void WriteTypedPrimitive (string name, string ns, object o, bool xsiType)
		{
			if (!xsiType) {
				WriteElementString (name, ns, o.ToString ());
				return;
			}

			XmlQualifiedName xsiTypeQName = new XmlQualifiedName (o.GetType ().Name, XmlSchema.Namespace);
			WriteElementString (name, ns, o.ToString (), xsiTypeQName);
		}

		protected void WriteValue (byte[] value)
		{
			Writer.WriteBase64 (value, 0, value.Length);
		}

		protected void WriteValue (string value)
		{
			Writer.WriteString (value);
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

		[MonoTODO ("Change prefix")]
		protected void WriteXsiType (string name, string ns)
		{
			WriteAttribute ("d0p1", "type", null, GetQualifiedName (name, ns).ToString ());
		}
		


		#endregion
	}
}
