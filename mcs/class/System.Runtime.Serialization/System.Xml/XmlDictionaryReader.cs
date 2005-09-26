#if NET_2_0
using System;
using System.IO;
using System.Xml;

namespace System.Xml
{
	public abstract class XmlDictionaryReader : XmlReader
	{
		protected XmlDictionaryReader ()
		{
		}

		public virtual bool CanCanonicalize {
			get { return false; }
		}

		public virtual bool CanGetContext {
			get { return false; }
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

		[MonoTODO]
		public XmlParserContext GetContext ()
		{
			throw new NotSupportedException ();
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
			XmlDictionaryString name;
			return TryGetLocalNameAsDictionaryString (out name) && object.ReferenceEquals (name, localName);
		}

		public virtual bool IsNamespaceUri (string namespaceUri)
		{
			return NamespaceURI == namespaceUri;
		}

		public virtual bool IsNamespaceUri (XmlDictionaryString namespaceUri)
		{
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
			XmlDictionaryString name;
			return TryGetNamespaceUriAsDictionaryString (out name) && object.ReferenceEquals (name, namespaceUri);
		}

		[MonoTODO]
		public bool IsStartArray (out Type type)
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
			XmlCanonicalWriter writer)
		{
			throw new NotSupportedException ();
		}

		[MonoTODO]
		public virtual bool TryGetArrayLength (out int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool TryGetBase64ContentLength (out int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool TryGetLocalNameAsDictionaryString (
			out XmlDictionaryString localName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool TryGetNamespaceUriAsDictionaryString (
			out XmlDictionaryString localName)
		{
			throw new NotImplementedException ();
		}

		// FIXME: add Read*Array() overloads


		#region Factory Methods

		public static XmlDictionaryReader CreateBinaryReader (
			byte [] buffer)
		{
			return CreateBinaryReader (buffer, 0, buffer.Length);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			byte [] buffer, int offset, int count)
		{
			return CreateBinaryReader (buffer, offset, count, new XmlDictionary ());
		}

		public static XmlDictionaryReader CreateBinaryReader (
			byte [] buffer, int offset, int count,
			IXmlDictionary dictionary)
		{
			return CreateBinaryReader (buffer, offset, count,
			dictionary, XmlDictionaryReaderQuotas .Default,
			new XmlBinaryReaderSession (), null, null);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			byte [] buffer, int offset, int count,
			IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quotas,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose,
			XmlParserContext context)
		{
			return CreateBinaryReader (new MemoryStream (
				buffer, offset, count), dictionary,
				quotas, session, onClose, context);
		}

		public static XmlDictionaryReader CreateBinaryReader (
			Stream stream)
		{
			return CreateBinaryReader (stream, new XmlDictionary ());
		}

		public static XmlDictionaryReader CreateBinaryReader (
			Stream stream, IXmlDictionary dictionary)
		{
			return CreateBinaryReader (stream, dictionary,
				XmlDictionaryReaderQuotas .Default,
				new XmlBinaryReaderSession (), null, null);
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateBinaryReader (
			Stream stream, IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quotas,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose,
			XmlParserContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlDictionaryReader CreateDictionaryReader (
			XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlDictionaryReader CreateDictionaryReader (
			XmlReader reader, bool isSoapCompliant)
		{
			throw new NotImplementedException ();
		}

		public XmlDictionaryReader CreateUTF8Reader (byte [] buffer)
		{
			return CreateUTF8Reader (buffer, 0, buffer.Length);
		}

		public static XmlDictionaryReader CreateUTF8Reader (
			byte [] buffer, int offset, int count)
		{
			return CreateUTF8Reader (buffer, offset, count, 
				XmlDictionaryReaderQuotas.Default,
				null, null);
		}

		[MonoTODO]
		public static XmlDictionaryReader CreateUTF8Reader (
			byte [] buffer, int offset, int count,
			XmlDictionaryReaderQuotas quotas,
			OnXmlDictionaryReaderClose onClose,
			XmlParserContext context)
		{
			throw new NotImplementedException ();
		}

		// FIXME: several factory methods here.

		#endregion
	}
}
#endif
