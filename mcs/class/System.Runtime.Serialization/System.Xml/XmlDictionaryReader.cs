//
// XmlDictionaryReader.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005, 2007 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace System.Xml
{
	public abstract partial class XmlDictionaryReader : XmlReader
	{
		protected XmlDictionaryReader ()
		{
		}

		XmlDictionaryReaderQuotas quotas;

		public virtual bool CanCanonicalize {
			get { return false; }
		}

		public virtual XmlDictionaryReaderQuotas Quotas {
			get {
				if (quotas == null)
					quotas = new XmlDictionaryReaderQuotas ();
				return quotas;
			}
		}

		public virtual void EndCanonicalization ()
		{
			throw new NotSupportedException ();
		}

		public virtual string GetAttribute (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
			return GetAttribute (localName.Value, namespaceUri.Value);
		}

		public virtual int IndexOfLocalName (
			string [] localNames, string namespaceUri)
		{
			if (localNames == null)
				throw new ArgumentNullException ("localNames");
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
			if (NamespaceURI != namespaceUri)
				return -1;
			for (int i = 0; i < localNames.Length; i++)
				if (localNames [i] == LocalName)
					return i;
			return -1;
		}

		public virtual int IndexOfLocalName (
			XmlDictionaryString [] localNames,
			XmlDictionaryString namespaceUri)
		{
			if (localNames == null)
				throw new ArgumentNullException ("localNames");
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
			if (NamespaceURI != namespaceUri.Value)
				return -1;
			XmlDictionaryString localName;
			if (!TryGetLocalNameAsDictionaryString (out localName))
				return -1;
			IXmlDictionary dict = localName.Dictionary;
			XmlDictionaryString iter;
			for (int i = 0; i < localNames.Length; i++)
				if (dict.TryLookup (localNames [i], out iter) && object.ReferenceEquals (iter, localName))
					return i;
			return -1;
		}

		public virtual bool IsArray (out Type type)
		{
			type = null;
			return false;
		}

		public virtual bool IsLocalName (string localName)
		{
			return LocalName == localName;
		}

		public virtual bool IsLocalName (XmlDictionaryString localName)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			return LocalName == localName.Value;
		}

		public virtual bool IsNamespaceUri (string namespaceUri)
		{
			return NamespaceURI == namespaceUri;
		}

		public virtual bool IsNamespaceUri (XmlDictionaryString namespaceUri)
		{
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
			return NamespaceURI == namespaceUri.Value;
		}

		public virtual bool IsStartArray (out Type type)
		{
			type = null;
			return false;
		}

		public virtual bool IsStartElement (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
			return IsStartElement (localName.Value, namespaceUri.Value);
		}

		protected bool IsTextNode (XmlNodeType nodeType)
		{
			switch (nodeType) {
			case XmlNodeType.Attribute: // wow, it isn't indeed.
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				return true;
			default:
				return false;
			}
		}

		XmlException XmlError (string message)
		{
			IXmlLineInfo li = this as IXmlLineInfo;
			if (li == null || !li.HasLineInfo ())
				return new XmlException (message);
			else
				return new XmlException (String.Format ("{0} in {1} , at ({2},{3})", message, BaseURI, li.LineNumber, li.LinePosition));
		}

		public virtual void MoveToStartElement ()
		{
			MoveToContent ();
			if (NodeType != XmlNodeType.Element)
				throw XmlError (String.Format ("Element node is expected, but got {0} node.", NodeType));
		}

		public virtual void MoveToStartElement (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			MoveToStartElement ();
			if (Name != name)
				throw XmlError (String.Format ("Element node '{0}' is expected, but got '{1}' element.", name, Name));
		}

		public virtual void MoveToStartElement (
			string localName, string namespaceUri)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
			MoveToStartElement ();
			if (LocalName != localName || NamespaceURI != namespaceUri)
				throw XmlError (String.Format ("Element node '{0}' in namespace '{1}' is expected, but got '{2}' in namespace '{3}' element.", localName, namespaceUri, LocalName, NamespaceURI));
		}

		public virtual void MoveToStartElement (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
			MoveToStartElement (localName.Value, namespaceUri.Value);
		}

		public virtual void StartCanonicalization (
			Stream stream, bool includeComments,
			string [] inclusivePrefixes)
		{
			throw new NotSupportedException ();
		}

		public virtual bool TryGetArrayLength (out int count)
		{
			count = -1;
			return false;
		}

		public virtual bool TryGetBase64ContentLength (out int count)
		{
			count = -1;
			return false;
		}

		public virtual bool TryGetLocalNameAsDictionaryString (
			out XmlDictionaryString localName)
		{
			localName = null;
			return false;
		}

		public virtual bool TryGetNamespaceUriAsDictionaryString (
			out XmlDictionaryString namespaceUri)
		{
			namespaceUri = null;
			return false;
		}

		#region Content Reader Methods

		public override object ReadContentAs (Type type, IXmlNamespaceResolver nsResolver)
		{
			return base.ReadContentAs (type, nsResolver);
		}

		public virtual byte [] ReadContentAsBase64 ()
		{
			int len;
			if (!TryGetBase64ContentLength (out len))
				return Convert.FromBase64String (ReadContentAsString ());
			byte [] bytes = new byte [len];
			ReadContentAsBase64 (bytes, 0, len);
			return bytes;
		}

		MethodInfo xmlconv_from_bin_hex = typeof (XmlConvert).GetMethod ("FromBinHexString", BindingFlags.Static | BindingFlags.NonPublic, null, new Type [] {typeof (string)}, null);

		byte [] FromBinHexString (string s)
		{
			return (byte []) xmlconv_from_bin_hex.Invoke (null, new object [] {s});
		}

		public virtual byte [] ReadContentAsBinHex ()
		{
			int len;
			if (!TryGetArrayLength (out len))
				return FromBinHexString (ReadContentAsString ());
			return ReadContentAsBinHex (len);
		}

		protected byte [] ReadContentAsBinHex (int maxByteArrayContentLength)
		{
			byte [] bytes = new byte [maxByteArrayContentLength];
			ReadContentAsBinHex (bytes, 0, maxByteArrayContentLength);
			return bytes;
		}

		[MonoTODO]
		public virtual int ReadContentAsChars (char [] chars, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override decimal ReadContentAsDecimal ()
		{
			return base.ReadContentAsDecimal ();
		}

		public override float ReadContentAsFloat ()
		{
			return base.ReadContentAsFloat ();
		}

		public virtual Guid ReadContentAsGuid ()
		{
			return XmlConvert.ToGuid (ReadContentAsString ());
		}

		public virtual void ReadContentAsQualifiedName (out string localName, out string namespaceUri)
		{
			XmlQualifiedName qname = (XmlQualifiedName) ReadContentAs (typeof (XmlQualifiedName), this as IXmlNamespaceResolver);
			localName = qname.Name;
			namespaceUri = qname.Namespace;
		}

		public override string ReadContentAsString ()
		{
			return ReadContentAsString (Quotas.MaxStringContentLength);
		}

		[MonoTODO]
		protected string ReadContentAsString (int maxStringContentLength)
		{
			return base.ReadContentAsString ();
		}

		[MonoTODO ("there is exactly no information on the web")]
		public virtual string ReadContentAsString (string [] strings, out int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("there is exactly no information on the web")]
		public virtual string ReadContentAsString (XmlDictionaryString [] strings, out int index)
		{
			throw new NotImplementedException ();
		}

		public virtual TimeSpan ReadContentAsTimeSpan ()
		{
			return XmlConvert.ToTimeSpan (ReadContentAsString ());
		}

		public virtual UniqueId ReadContentAsUniqueId ()
		{
			return new UniqueId (ReadContentAsString ());
		}

		public virtual byte [] ReadElementContentAsBase64 ()
		{
			ReadStartElement ();
			byte [] ret = ReadContentAsBase64 ();
			ReadEndElement ();
			return ret;
		}

		public virtual byte [] ReadElementContentAsBinHex ()
		{
			ReadStartElement ();
			byte [] ret = ReadContentAsBinHex ();
			ReadEndElement ();
			return ret;
		}

		public virtual Guid ReadElementContentAsGuid ()
		{
			ReadStartElement ();
			Guid ret = ReadContentAsGuid ();
			ReadEndElement ();
			return ret;
		}

		public virtual TimeSpan ReadElementContentAsTimeSpan ()
		{
			ReadStartElement ();
			TimeSpan ret = ReadContentAsTimeSpan ();
			ReadEndElement ();
			return ret;
		}

		public virtual UniqueId ReadElementContentAsUniqueId ()
		{
			ReadStartElement ();
			UniqueId ret = ReadContentAsUniqueId ();
			ReadEndElement ();
			return ret;
		}

		public override string ReadElementContentAsString ()
		{
			if (IsEmptyElement) {
				Read ();
				return String.Empty;
			} else {
				ReadStartElement ();
				string s;
				if (NodeType == XmlNodeType.EndElement)
					s = String.Empty;
				else
					s = ReadContentAsString ();
				ReadEndElement ();
				return s;
			}
		}

		public virtual void ReadFullStartElement ()
		{
			if (!IsStartElement ())
				throw new XmlException ("Current node is not a start element");
			ReadStartElement ();
		}

		public virtual void ReadFullStartElement (string name)
		{
			if (!IsStartElement (name))
				throw new XmlException (String.Format ("Current node is not a start element '{0}'", name));
			ReadStartElement (name);
		}

		public virtual void ReadFullStartElement (string localName, string namespaceUri)
		{
			if (!IsStartElement (localName, namespaceUri))
				throw new XmlException (String.Format ("Current node is not a start element '{0}' in namesapce '{1}'", localName, namespaceUri));
			ReadStartElement (localName, namespaceUri);
		}

		public virtual void ReadFullStartElement (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			if (!IsStartElement (localName, namespaceUri))
				throw new XmlException (String.Format ("Current node is not a start element '{0}' in namesapce '{1}'", localName, namespaceUri));
			ReadStartElement (localName.Value, namespaceUri.Value);
		}

		public virtual void ReadStartElement (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
			ReadStartElement (localName.Value, namespaceUri.Value);
		}

		public override string ReadString ()
		{
			return ReadString (Quotas.MaxStringContentLength);
		}

		[MonoTODO]
		protected string ReadString (int maxStringContentLength)
		{
			return base.ReadString ();
		}

		public virtual int ReadValueAsBase64 (byte [] bytes, int start, int length)
		{
			throw new NotSupportedException (); // as it is documented ...
		}

		public virtual bool TryGetValueAsDictionaryString (out XmlDictionaryString value)
		{
			throw new NotSupportedException (); // as documented
		}

		#endregion

		#region Factory Methods

		public static XmlDictionaryReader CreateBinaryReader (
			byte [] buffer, XmlDictionaryReaderQuotas quotas)
		{
			return CreateBinaryReader (buffer, 0, buffer.Length, quotas);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			byte [] buffer, int offset, int count, 
			XmlDictionaryReaderQuotas quotas)
		{
			return CreateBinaryReader (buffer, offset, count, new XmlDictionary (), quotas);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			byte [] buffer, int offset, int count,
			IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quotas)
		{
			return CreateBinaryReader (buffer, offset, count,
				dictionary, quotas,
				new XmlBinaryReaderSession (), null);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			byte [] buffer, int offset, int count,
			IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quotas,
			XmlBinaryReaderSession session)
		{
			return CreateBinaryReader (buffer, offset, count,
				dictionary, quotas,
				session, null);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			byte [] buffer, int offset, int count,
			IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quotas,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose)
		{
			return new XmlBinaryDictionaryReader (buffer,
				offset, count,
				dictionary, quotas, session, onClose);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			Stream stream, XmlDictionaryReaderQuotas quotas)
		{
			return CreateBinaryReader (stream, new XmlDictionary (), quotas);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			Stream stream, IXmlDictionary dictionary, 
			XmlDictionaryReaderQuotas quotas)
		{
			return CreateBinaryReader (stream, dictionary, quotas,
				new XmlBinaryReaderSession (), null);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			Stream stream, IXmlDictionary dictionary, 
			XmlDictionaryReaderQuotas quotas,
			XmlBinaryReaderSession session)
		{
			return CreateBinaryReader (stream, dictionary, quotas,
				session, null);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			Stream stream, IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quotas,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose)
		{
			return new XmlBinaryDictionaryReader (stream,
				dictionary, quotas, session, onClose);
		}

		public static XmlDictionaryReader CreateDictionaryReader (
			XmlReader reader)
		{
			return new XmlSimpleDictionaryReader (reader);
		}

#if !NET_2_1
		public static XmlDictionaryReader CreateMtomReader (
			Stream stream, Encoding encoding,
			XmlDictionaryReaderQuotas quotas)
		{
			return new XmlMtomDictionaryReader (stream, encoding, quotas);
		}

		public static XmlDictionaryReader CreateMtomReader (
			Stream stream, Encoding [] encodings,
			XmlDictionaryReaderQuotas quotas)
		{
			return CreateMtomReader (stream, encodings, null, quotas);
		}

		public static XmlDictionaryReader CreateMtomReader (
			Stream stream, Encoding [] encodings, string contentType,
			XmlDictionaryReaderQuotas quotas)
		{
			return CreateMtomReader (stream, encodings, contentType, quotas, int.MaxValue, null);
		}

		public static XmlDictionaryReader CreateMtomReader (
			Stream stream, Encoding [] encodings, string contentType,
			XmlDictionaryReaderQuotas quotas,
			int maxBufferSize,
			OnXmlDictionaryReaderClose onClose)
		{
			return new XmlMtomDictionaryReader (stream, encodings, contentType, quotas, maxBufferSize, onClose);
		}

		public static XmlDictionaryReader CreateMtomReader (
			byte [] buffer, int offset, int count,
			Encoding encoding, XmlDictionaryReaderQuotas quotas)
		{
			return CreateMtomReader (new MemoryStream (buffer, offset, count), encoding, quotas);
		}

		public static XmlDictionaryReader CreateMtomReader (
			byte [] buffer, int offset, int count,
			Encoding [] encodings, XmlDictionaryReaderQuotas quotas)
		{
			return CreateMtomReader (new MemoryStream (buffer, offset, count), encodings, quotas);
		}

		public static XmlDictionaryReader CreateMtomReader (
			byte [] buffer, int offset, int count,
			Encoding [] encodings, string contentType,
			XmlDictionaryReaderQuotas quotas)
		{
			return CreateMtomReader (new MemoryStream (buffer, offset, count), encodings, contentType, quotas);
		}

		public static XmlDictionaryReader CreateMtomReader (
			byte [] buffer, int offset, int count,
			Encoding [] encodings, string contentType,
			XmlDictionaryReaderQuotas quotas,
			int maxBufferSize,
			OnXmlDictionaryReaderClose onClose)
		{
			return CreateMtomReader (new MemoryStream (buffer, offset, count), encodings, contentType, quotas, maxBufferSize, onClose);
		}
#endif

		public static XmlDictionaryReader CreateTextReader (byte [] buffer, XmlDictionaryReaderQuotas quotas)
		{
			return CreateTextReader (buffer, 0, buffer.Length, quotas);
		}

		public static XmlDictionaryReader CreateTextReader (
			byte [] buffer, int offset, int count,
			XmlDictionaryReaderQuotas quotas)
		{
			return CreateTextReader (buffer, offset, count,
				Encoding.UTF8, quotas, null);
		}

		public static XmlDictionaryReader CreateTextReader (
			byte [] buffer, int offset, int count,
			Encoding encoding,
			XmlDictionaryReaderQuotas quotas,
			OnXmlDictionaryReaderClose onClose)
		{
			return CreateTextReader (new MemoryStream (buffer, offset, count), encoding, quotas, onClose);
		}

		public static XmlDictionaryReader CreateTextReader (
			Stream stream, XmlDictionaryReaderQuotas quotas)
		{
			return CreateTextReader (stream, Encoding.UTF8, quotas, null);
		}

		public static XmlDictionaryReader CreateTextReader (
			Stream stream, Encoding encoding,
			XmlDictionaryReaderQuotas quotas,
			OnXmlDictionaryReaderClose onClose)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			XmlNameTable nt = new NameTable ();
			XmlParserContext c = new XmlParserContext (nt, new XmlNamespaceManager (nt), String.Empty, XmlSpace.None, encoding);
			XmlDictionaryReader res = new XmlSimpleDictionaryReader (XmlReader.Create (stream, s, c), null, onClose);
			res.quotas = quotas;
			return res;
		}

		#endregion
	}
}
