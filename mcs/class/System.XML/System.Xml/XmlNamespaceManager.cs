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
			
		HighWaterStack decls = new HighWaterStack (50);
		HighWaterStack scopes = new HighWaterStack (50);
		Namespace defaultNamespace;
		int count = 0;
		
		internal const string XmlnsXml = "http://www.w3.org/XML/1998/namespace";
		internal const string XmlnsXmlns = "http://www.w3.org/2000/xmlns/";

		string XMLNS, XML, XMLNS_URL, XML_URL;
		#endregion

		#region Constructor

		public XmlNamespaceManager (XmlNameTable nameTable)
		{
			this.nameTable = nameTable;

			XMLNS = nameTable.Add ("xmlns");
			XML = nameTable.Add ("xml");
			XMLNS_URL = nameTable.Add (XmlnsXmlns);
			XML_URL = nameTable.Add (XmlnsXml);
		}

		#endregion

		#region Properties

		public virtual string DefaultNamespace {
			get { return (defaultNamespace == null) ? String.Empty : defaultNamespace.Uri; }
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

			prefix = nameTable.Add (prefix);
			uri = nameTable.Add (uri);

			// Is it already in the table?
			for (int i = decls.Length - 1; i >= decls.Length - count; i--) {
				Namespace decl = (Namespace)decls [i];
				if (AtomStrEq (decl.Prefix, prefix)) {
					// Then redefine it
					decl.Uri = uri;
					return;
				}
			}
			
			// Otherwise, we are going to add it as a new object
			Namespace newDecl = (Namespace) decls.Push ();
			if (newDecl == null) {
				newDecl = new Namespace ();
				decls.AddToTop (newDecl);
			}
			newDecl.Prefix = prefix;
			newDecl.Uri = uri;
			count++;
			if (prefix == String.Empty)
				defaultNamespace = newDecl;
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
			Hashtable p = new Hashtable (count);
			for (int i = decls.Length - 1; i >= decls.Length - count; i--) {
				Namespace decl = (Namespace)decls [i];
				if (decl.Prefix != String.Empty && decl.Uri != null)
					p [decl.Prefix] = decl.Uri;
			}
			p [String.Empty] = DefaultNamespace;
			p [XML] = XML_URL;
			p [XMLNS] = XMLNS_URL;
			return p.Keys.GetEnumerator ();
		}

		public virtual bool HasNamespace (string prefix)
		{
			if (prefix == null) return false;
			
			for (int i = decls.Length - 1; i >= decls.Length - count; i--) {
				Namespace decl = (Namespace)decls [i];
				if (AtomStrEq (decl.Prefix, prefix) && decl.Uri != null)
					return true;
			}
			return false;
		}

		public virtual string LookupNamespace (string prefix)
		{
			if (prefix == null)
				return null;

			if (prefix == String.Empty)
				return DefaultNamespace;

			if (AtomStrEq (XML, prefix))
				return XML_URL;

			if (AtomStrEq (XMLNS, prefix))
				return XMLNS_URL;

			for (int i = decls.Length - 1; i >= 0; i--) {
				Namespace decl = (Namespace)decls [i];
				if (AtomStrEq (decl.Prefix, prefix) && decl.Uri != null)
					return decl.Uri;
			}
			return null;
		}


		public virtual string LookupPrefix (string uri)
		{
			if (uri == null)
				return null;

			if (AtomStrEq (DefaultNamespace, uri))
				return String.Empty;
			
			if (AtomStrEq (XML_URL, uri))
				return XML;
			
			if (AtomStrEq (XMLNS_URL, uri))
				return XMLNS;


			for (int i = decls.Length - 1; i >= 0; i--) {
				Namespace decl = (Namespace)decls [i];
				if (AtomStrEq (decl.Uri, uri) && decl.Uri != null)
					return decl.Prefix;
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
			Scope current = (Scope)scopes.Pop ();
			if (current == null) {
				return false;
			} else {
				for (int i = 0; i < count; i++)
					decls.Pop ();
					
				defaultNamespace = current.DefaultNamespace;
				count = current.Count;
				return true;
			}
		}

		public virtual void PushScope ()
		{
			Scope current = (Scope)scopes.Push ();
			if (current == null) {
				current = new Scope ();
				scopes.AddToTop (current);
			}
			current.DefaultNamespace = defaultNamespace;
			current.Count = count;
			count = 0;
		}

		public virtual void RemoveNamespace (string prefix, string uri)
		{
			if (prefix == null)
				throw new ArgumentNullException ("prefix");

			if (uri == null)
				throw new ArgumentNullException ("uri");

			string p = nameTable.Get (prefix);
			string u = nameTable.Get (uri);
			if (p == null || u == null)
				return;
				
			for (int i = decls.Length - 1; i >= decls.Length - count; i--) {
				Namespace n = (Namespace)decls [i];
				if (AtomStrEq (n.Prefix, p) && AtomStrEq (n.Uri, u))
					n.Uri = null;
			}
		}
		
		bool AtomStrEq (string a, string b) {
			if (String.Equals (a, b) && !Object.ReferenceEquals (a, b)) {
				Console.Error.WriteLine ("WARNING: {0} not interned", a);
			}
			
			return String.Equals (a, b);

		}

		#endregion
		class Namespace {
			public string Prefix, Uri;
	}

		class Scope {
			public Namespace DefaultNamespace;
			public int Count;
		}
	}
}
