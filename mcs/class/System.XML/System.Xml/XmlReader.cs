//
// XmlReader.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
//

namespace System.Xml
{
	public abstract class XmlReader
	{
		#region Constructor

		protected XmlReader ()
		{
		}

		#endregion

		#region Properties

		public abstract int AttributeCount { get; }

		public abstract string BaseURI { get; }

		public virtual bool CanResolveEntity
		{
			get	{ return false; }
		}

		public abstract int Depth { get; }

		public abstract bool EOF { get; }

		public virtual bool HasAttributes
		{
			get { return AttributeCount > 0; }
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

		#endregion

		#region Methods

		public abstract void Close ();

		public abstract string GetAttribute (int i);

		public abstract string GetAttribute (string name);

		public abstract string GetAttribute (
			string localName,
			string namespaceName);

		public static bool IsName (string s)
		{
			bool result = false;

			if (s != null && s.Length > 0) {
				char[] chars = s.ToCharArray ();

				if (XmlChar.IsFirstNameChar (chars[0])) {
					int i = 1;
					int n = chars.Length;

					while (i < n && XmlChar.IsNameChar (chars[i]))
						++i;

					result = i == n;
				}
			}

			return result;
		}

		public static bool IsNameToken (string s)
		{
			bool result = false;

			if (s != null && s.Length > 0) {
				char[] chars = s.ToCharArray ();

				int i = 0;
				int n = chars.Length;

				while (i < n && XmlChar.IsNameChar (chars[i]))
					++i;

				result = i == n;
			}

			return result;
		}

		[MonoTODO]
		public virtual bool IsStartElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsStartElement (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsStartElement (
			string localName,
			string namespaceName)
		{
			throw new NotImplementedException ();
		}

		public abstract string LookupNamespace (string prefix);

		public abstract void MoveToAttribute (int i);

		public abstract bool MoveToAttribute (string name);

		public abstract bool MoveToAttribute (
			string localName,
			string namespaceName);

		[MonoTODO]
		public virtual XmlNodeType MoveToContent ()
		{
			throw new NotImplementedException ();
		}

		public abstract bool MoveToElement ();

		public abstract bool MoveToFirstAttribute ();

		public abstract bool MoveToNextAttribute ();

		public abstract bool Read ();

		public abstract bool ReadAttributeValue ();

		[MonoTODO]
		public virtual string ReadElementString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string ReadElementString (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string ReadElementString (
			string localName,
			string namespaceName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ReadEndElement ()
		{
			throw new NotImplementedException ();
		}

		public abstract string ReadInnerXml ();

		public abstract string ReadOuterXml ();

		[MonoTODO]
		public virtual void ReadStartElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ReadStartElement (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ReadStartElement (
			string localName,
			string namespaceName)
		{
			throw new NotImplementedException ();
		}

		public abstract string ReadString ();

		public abstract void ResolveEntity ();

		[MonoTODO]
		public virtual void Skip ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
