// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlNamespaceManager.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

using System.Collections;

namespace System.Xml
{
	public class XmlNamespaceManager : IEnumerable
	{
		private XmlNameTable _NameTable;
		NamespaceScope _Top;

		public XmlNamespaceManager(XmlNameTable nameTable)
		{
			_NameTable = nameTable;
			PushScope();
		}

		public virtual string DefaultNamespace
		{
			get
			{
				return LookupNamespace(String.Empty);
			}
		}

		public XmlNameTable NameTable
		{
			get
			{
				return _NameTable;
			}
		}

		public virtual void AddNamespace(string prefix, string uri)
		{
			if (prefix == null)
			{
				throw new ArgumentNullException("prefix", "Value cannot be null.");
			}

			if (uri == null)
			{
				throw new ArgumentNullException("uri", "Value cannot be null.");
			}

			if (prefix.Length > 2 && prefix.Substring(0, 3).ToLower() == "xml")
			{
				throw new ArgumentException("Prefixes beginning with \"xml\" (regardless of whether the characters are uppercase, lowercase, or some combination thereof) are reserved for use by XML.", "prefix");
			}

			if (_Top.Namespaces == null)
			{
				_Top.Namespaces = new Hashtable();
			}

			_Top.Namespaces.Add(prefix, uri);
		}

		public virtual IEnumerator GetEnumerator()
		{
			// TODO: implement me.
			throw new NotImplementedException();
		}

		public virtual bool HasNamespace(string prefix)
		{
			return _Top != null && _Top.Namespaces != null && _Top.Namespaces.Contains(prefix);
		}

		public virtual string LookupNamespace(string prefix)
		{
			NamespaceScope scope = _Top;

			while (scope != null)
			{
				if (scope.Namespaces != null && scope.Namespaces.Contains(prefix))
				{
					return scope.Namespaces[prefix] as string;
				}

				scope = scope.Next;
			}

			switch (prefix)
			{
				case "xmlns":
					return "http://www.w3.org/2000/xmlns/";
				case "xml":
					return "http://www.w3.org/XML/1998/namespace";
				case "":
					return String.Empty;
			}

			return null;
		}

		public virtual string LookupPrefix(string uri)
		{
			// TODO: implement me.
			throw new NotImplementedException();
		}

		public virtual bool PopScope()
		{
			if (_Top != null)
			{
				_Top = _Top.Next;
				return true;
			}

			return false;
		}

		public virtual void PushScope()
		{
			NamespaceScope newScope = new NamespaceScope();
			newScope.Next = _Top;
			_Top = newScope;
		}

		public virtual void RemoveNamespace(string prefix, string uri)
		{
			// TODO: implement me.
			throw new NotImplementedException();
		}
	}

	internal class NamespaceScope
	{
		internal NamespaceScope Next;
		internal Hashtable Namespaces;
	}
}
