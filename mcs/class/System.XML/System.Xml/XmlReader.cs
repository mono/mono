// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlReader.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

namespace System.Xml
{
	public abstract class XmlReader
	{
		// properties

		public abstract int AttributeCount { get; }

		public abstract string BaseURI { get; }

		public virtual bool CanResolveEntity
		{
			get
			{
				return false;
			}
		}

		public abstract int Depth { get; }

		public abstract bool EOF { get; }

		public virtual bool HasAttributes
		{
			get
			{
				return AttributeCount > 0;
			}
		}

		public abstract bool HasValue { get; }

		public abstract bool IsDefault { get; }

		public abstract bool IsEmptyElement { get; }

		public abstract string this[int i] { get; }

		public abstract string this[string name] { get; }

		public abstract string this[
			string localName,
			string namespaceName]
		{ get; }

		public abstract string LocalName { get; }

		public abstract string Name { get; }

		public abstract string NamespaceURI { get; }

		public abstract XmlNameTable NameTable { get; }

		public abstract XmlNodeType NodeType { get; }

		public abstract string Prefix { get; }

		public abstract char QuoteChar { get; }

		public abstract ReadState ReadState { get; }

		public abstract string Value { get; }

		public abstract string XmlLang { get; }

		public abstract XmlSpace XmlSpace { get; }

		// methods

		public abstract void Close();

		public abstract string GetAttribute(int i);

		public abstract string GetAttribute(string name);

		public abstract string GetAttribute(
			string localName,
			string namespaceName);

		public static bool IsName(string s)
		{
			bool result = false;

			if (s != null && s.Length > 0)
			{
				char[] chars = s.ToCharArray();

				if (XmlChar.IsFirstNameChar(chars[0]))
				{
					int i = 1;
					int n = chars.Length;

					while (i < n && XmlChar.IsNameChar(chars[i]))
					{
						++i;
					}

					result = i == n;
				}
			}

			return result;
		}

		public static bool IsNameToken(string s)
		{
			bool result = false;

			if (s != null && s.Length > 0)
			{
				char[] chars = s.ToCharArray();

				int i = 0;
				int n = chars.Length;

				while (i < n && XmlChar.IsNameChar(chars[i]))
				{
					++i;
				}

				result = i == n;
			}

			return result;
		}

		public virtual bool IsStartElement()
		{
			// TODO: implement me.
			return false;
		}

		public virtual bool IsStartElement(string name)
		{
			// TODO: implement me.
			return false;
		}

		public virtual bool IsStartElement(
			string localName,
			string namespaceName)
		{
			// TODO: implement me.
			return false;
		}

		public abstract string LookupNamespace(string prefix);

		public abstract void MoveToAttribute(int i);

		public abstract bool MoveToAttribute(string name);

		public abstract bool MoveToAttribute(
			string localName,
			string namespaceName);

		public virtual XmlNodeType MoveToContent()
		{
			// TODO: implement me.
			return XmlNodeType.None;
		}

		public abstract bool MoveToElement();

		public abstract bool MoveToFirstAttribute();

		public abstract bool MoveToNextAttribute();

		public abstract bool Read();

		public abstract bool ReadAttributeValue();

		public virtual string ReadElementString()
		{
			// TODO: implement me.
			return null;
		}

		public virtual string ReadElementString(string name)
		{
			// TODO: implement me.
			return null;
		}

		public virtual string ReadElementString(
			string localName,
			string namespaceName)
		{
			// TODO: implement me.
			return null;
		}

		public virtual void ReadEndElement()
		{
			// TODO: implement me.
		}

		public abstract string ReadInnerXml();

		public abstract string ReadOuterXml();

		public virtual void ReadStartElement()
		{
			// TODO: implement me.
		}

		public virtual void ReadStartElement(string name)
		{
			// TODO: implement me.
		}

		public virtual void ReadStartElement(
			string localName,
			string namespaceName)
		{
			// TODO: implement me.
		}

		public abstract string ReadString();

		public abstract void ResolveEntity();

		public virtual void Skip()
		{
			// TODO: implement me.
		}
	}
}
