using System;
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

		// FIXME: several factory methods here.

		public virtual void EndCanonicalization ()
		{
			throw new NotImplementedException ();
		}

		public virtual string GetAttribute (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			throw new NotImplementedException ();
		}

		public XmlParserContext GetContext ()
		{
			throw new NotImplementedException ();
		}

		public virtual int IndexOfLocalName (
			string [] localNames, string namespaceUri)
		{
			throw new NotImplementedException ();
		}

		public virtual int IndexOfLocalName (
			XmlDictionaryString [] localNames,
			XmlDictionaryString namespaceUri)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsNamespaceUri (string namespaceUri)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsNamespaceUri (XmlDictionaryString namespaceUri)
		{
			throw new NotImplementedException ();
		}

		public bool IsStartArray (out Type type)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsStartElement (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			throw new NotImplementedException ();
		}

		public virtual void MoveToStartElement (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			throw new NotImplementedException ();
		}

		// FIXME: add Read*Array() overloads

		public virtual void StartCanonicalization (
			XmlCanonicalWriter writer)
		{
			throw new NotImplementedException ();
		}

		public virtual bool TryGetArrayLength (out int count)
		{
			throw new NotImplementedException ();
		}

		public virtual bool TryGetBase64ContentLength (out int count)
		{
			throw new NotImplementedException ();
		}

		public virtual bool TryGetLocalNameAsDictionaryString (
			out XmlDictionaryString localName)
		{
			throw new NotImplementedException ();
		}

		public virtual bool TryGetNamespaceUriAsDictionaryString (
			out XmlDictionaryString localName)
		{
			throw new NotImplementedException ();
		}
	}
}
