//
// XmlNamespaceManager.cs
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
		#region Fields

		private XmlNameTable nameTable;
		private NamespaceScope currentScope;
		internal const string XmlnsXml = "http://www.w3.org/XML/1998/namespace";
		internal const string XmlnsXmlns = "http://www.w3.org/2000/xmlns/";

		#endregion

		#region Constructor

		public XmlNamespaceManager (XmlNameTable nameTable)
		{
			this.nameTable = nameTable;

			nameTable.Add ("xmlns");
			nameTable.Add ("xml");
			nameTable.Add (String.Empty);
			nameTable.Add (XmlnsXmlns);
			nameTable.Add (XmlnsXml);

			PushScope ();
			currentScope.Namespaces = new Hashtable ();
			currentScope.Namespaces.Add ("xml", XmlnsXml);
			currentScope.Namespaces.Add ("xmlns", XmlnsXmlns);
		}

		#endregion

		#region Properties

		public virtual string DefaultNamespace {
			get { return LookupNamespace (String.Empty); }
		}

		public XmlNameTable NameTable {
			get { return nameTable; }
		}

		#endregion

		#region Methods

		public virtual void AddNamespace (string prefix, string uri)
		{
			if (prefix == null)
				throw new ArgumentNullException ("prefix", "Value cannot be null.");

			if (uri == null)
				throw new ArgumentNullException ("uri", "Value cannot be null.");

			IsValidDeclaration (prefix, uri, true);

			if (currentScope.Namespaces == null)
				currentScope.Namespaces = new Hashtable ();

			if (prefix != String.Empty)
				nameTable.Add (prefix);
			currentScope.Namespaces [prefix] = nameTable.Add (uri);
		}

		internal static string IsValidDeclaration (string prefix, string uri, bool throwException)
		{
			string message = null;
			if (prefix == "xml" && uri != XmlnsXml)
				message = String.Format ("Prefix \"xml\" is only allowed to the fixed uri \"{0}\"", XmlnsXml);
			else if (uri == XmlnsXml)
				message = String.Format ("Namespace URI \"{0}\" can only be declared with the fixed prefix \"xml\"", XmlnsXml);
			if (message == null && prefix == "xmlns")
				message = "Declaring prefix named \"xmlns\" is not allowed to any namespace.";
			if (message == null && uri == XmlnsXmlns)
				message = String.Format ("Namespace URI \"{0}\" cannot be declared with any namespace.", XmlnsXmlns);
			if (message != null && throwException)
				throw new ArgumentException (message);
			else
				return message;
		}

		public virtual IEnumerator GetEnumerator ()
		{
			if (currentScope.Namespaces == null)
				currentScope.Namespaces = new Hashtable ();

			return currentScope.Namespaces.Keys.GetEnumerator ();
		}

		public virtual bool HasNamespace (string prefix)
		{
			return currentScope != null && currentScope.Namespaces != null && currentScope.Namespaces.Contains (prefix);
		}

		public virtual string LookupNamespace (string prefix)
		{
			NamespaceScope scope = currentScope;

			while (scope != null) {
				if (scope.Namespaces != null && scope.Namespaces.Contains (prefix))
					return scope.Namespaces[prefix] as string;
				scope = scope.Next;
			}

			switch (prefix) {
			case "xmlns":
				return nameTable.Get (XmlnsXmlns);
			case "xml":
				return nameTable.Get (XmlnsXml);
			case "":
				return nameTable.Get (String.Empty);
			}

			return null;
		}

		public virtual string LookupPrefix (string uri)
		{
			if (uri == null)
				return null;

			NamespaceScope scope = currentScope;

			while (scope != null) 
			{
				if (scope.Namespaces != null && scope.Namespaces.ContainsValue (uri)) {
					foreach (DictionaryEntry entry in scope.Namespaces) {
						if (entry.Value.ToString() == uri)
							return nameTable.Get (entry.Key as string) as string;
					}
				}

				scope = scope.Next;
			}

			// ECMA specifies that this method returns String.Empty
			// in case of no match. But actually MS.NET returns null.
			// For more information,see
			//  http://lists.ximian.com/archives/public/mono-list/2003-January/005071.html
			//return String.Empty;
			return null;
		}

		public virtual bool PopScope ()
		{
			if (currentScope != null)
				currentScope = currentScope.Next;

			return currentScope != null;
		}

		public virtual void PushScope ()
		{
			NamespaceScope newScope = new NamespaceScope ();
			newScope.Next = currentScope;
			currentScope = newScope;
		}

		public virtual void RemoveNamespace (string prefix, string uri)
		{
			if (prefix == null)
				throw new ArgumentNullException ("prefix");

			if (uri == null)
				throw new ArgumentNullException ("uri");

			if (currentScope == null || currentScope.Namespaces == null)
				return;

			string p = nameTable.Get (prefix);
			string u = nameTable.Get (uri);
			if (p == null || u == null)
				return;
				
			string storedUri = currentScope.Namespaces [p] as string;
			if (storedUri == null || storedUri != u)
				return;

			currentScope.Namespaces.Remove (p);
		}

		#endregion
	}

	internal class NamespaceScope
	{
		internal NamespaceScope Next;
		internal Hashtable Namespaces;
	}
}
