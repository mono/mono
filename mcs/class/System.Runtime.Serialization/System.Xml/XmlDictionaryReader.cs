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
#if NET_2_0
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

		[MonoTODO]
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

		[MonoTODO]
		public virtual bool IsStartArray (out Type type)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public virtual void MoveToStartElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void MoveToStartElement (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void MoveToStartElement (
			string localName, string namespaceUri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void MoveToStartElement (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		[MonoTODO]
		public virtual bool TryGetLocalNameAsDictionaryString (
			out XmlDictionaryString localName)
		{
			localName = null;
			return false;
		}

		[MonoTODO]
		public virtual bool TryGetNamespaceUriAsDictionaryString (
			out XmlDictionaryString namespaceUri)
		{
			namespaceUri = null;
			return false;
		}

		#region Content Reader Methods

		[MonoTODO]
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

		[MonoTODO]
		public override decimal ReadContentAsDecimal ()
		{
			return base.ReadContentAsDecimal ();
		}

		[MonoTODO]
		public override float ReadContentAsFloat ()
		{
			return base.ReadContentAsFloat ();
		}

		public virtual Guid ReadContentAsGuid ()
		{
			return XmlConvert.ToGuid (ReadContentAsString ());
		}

		[MonoTODO]
		public virtual void ReadContentAsQualifiedName (out string localName, out string namespaceUri)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public virtual string ReadContentAsString (string [] strings, out int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		public override string ReadString ()
		{
			return ReadString (Quotas.MaxStringContentLength);
		}

		[MonoTODO]
		protected string ReadString (int maxStringContentLength)
		{
			return base.ReadString ();
		}

		[MonoTODO]
		public virtual byte [] ReadValueAsBase64 ()
		{
			return ReadContentAsBase64 ();
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

		[MonoTODO]
		public static XmlDictionaryReader CreateBinaryReader (
			Stream stream, IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quotas,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose)
		{
			return new XmlBinaryDictionaryReader (stream,
				dictionary, quotas, session, onClose);
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateDictionaryReader (
			XmlReader reader)
		{
			return new XmlSimpleDictionaryReader (reader);
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateMtomReader (
			Stream stream, Encoding encoding,
			XmlDictionaryReaderQuotas quotas)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateMtomReader (
			Stream stream, Encoding [] encodings,
			XmlDictionaryReaderQuotas quotas)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateMtomReader (
			Stream stream, Encoding [] encodings, string contentType,
			XmlDictionaryReaderQuotas quotas)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateMtomReader (
			Stream stream, Encoding [] encodings, string contentType,
			XmlDictionaryReaderQuotas quotas,
			int maxBufferSize,
			OnXmlDictionaryReaderClose onClose)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateMtomReader (
			byte [] buffer, int offset, int count,
			Encoding encoding, XmlDictionaryReaderQuotas quotas)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateMtomReader (
			byte [] buffer, int offset, int count,
			Encoding [] encodings, XmlDictionaryReaderQuotas quotas)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateMtomReader (
			byte [] buffer, int offset, int count,
			Encoding [] encodings, string contentType,
			XmlDictionaryReaderQuotas quotas)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateMtomReader (
			byte [] buffer, int offset, int count,
			Encoding [] encodings, string contentType,
			XmlDictionaryReaderQuotas quotas,
			int maxBufferSize,
			OnXmlDictionaryReaderClose onClose)
		{
			throw new NotImplementedException ();
		}

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

		[MonoTODO]
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

		[MonoTODO]
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
#endif
