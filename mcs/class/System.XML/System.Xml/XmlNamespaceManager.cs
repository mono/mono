//
// XmlNamespaceManager.cs
//
// Authors:
//   Jason Diamond (jason@injektilo.org)
//   Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
// (C) 2003 Ben Maurer
//

using System.Collections;

namespace System.Xml
{
	public class XmlNamespaceManager : IEnumerable
	{
		#region Data
		struct NsDecl {
			public string Prefix, Uri;
		}
		
		struct NsScope {
			public int DeclCount;
			public string DefaultNamespace;
		}
		
		NsDecl [] decls;
		int declPos = -1;
		
		NsScope [] scopes;
		int scopePos = -1;
		
		string defaultNamespace;
		int count;
		
		void InitData ()
		{
			decls = new NsDecl [10];
			scopes = new NsScope [40];
		}
		
		// precondition declPos == nsDecl.Length
		void GrowDecls ()
		{
			NsDecl [] old = decls;
			decls = new NsDecl [declPos * 2 + 1];
			if (declPos > 0)
				Array.Copy (old, 0, decls, 0, declPos);
		}
		
		// precondition scopePos == scopes.Length
		void GrowScopes ()
		{
			NsScope [] old = scopes;
			scopes = new NsScope [scopePos * 2 + 1];
			if (scopePos > 0)
				Array.Copy (old, 0, scopes, 0, scopePos);
		}
		
		#endregion
		
		#region Fields

		private XmlNameTable nameTable;
		internal const string XmlnsXml = "http://www.w3.org/XML/1998/namespace";
		internal const string XmlnsXmlns = "http://www.w3.org/2000/xmlns/";

		#endregion

		#region Constructor

		internal XmlNamespaceManager () {}
		public XmlNamespaceManager (XmlNameTable nameTable)
		{
			this.nameTable = nameTable;

			nameTable.Add ("xmlns");
			nameTable.Add ("xml");
			nameTable.Add (String.Empty);
			nameTable.Add (XmlnsXmlns);
			nameTable.Add (XmlnsXml);
			
			InitData ();
		}

		#endregion

		#region Properties

		public virtual string DefaultNamespace {
			get { return defaultNamespace == null ? string.Empty : defaultNamespace; }
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
			
			prefix = nameTable.Add (prefix);
			uri = nameTable.Add (uri);

			IsValidDeclaration (prefix, uri, true);

			if (prefix == string.Empty)
				defaultNamespace = uri;
			
			for (int i = declPos; i > declPos - count; i--) {
				if (decls [i].Prefix == prefix) {
					decls [i].Uri = uri;
					return;
				}
			}
			
			declPos ++;
			count ++;
			
			if (declPos == decls.Length)
				GrowDecls ();
			decls [declPos].Prefix = prefix;
			decls [declPos].Uri = uri;
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
			// In fact it returns such table's enumerator that contains all the namespaces.
			// while HasNamespace() ignores pushed namespaces.
			
			Hashtable ht = new Hashtable ();
			for (int i = 0; i < declPos; i++) {
				if (decls [i].Prefix != string.Empty && decls [i].Uri != null) {
					ht [decls [i].Prefix] = decls [i].Uri;
				}
			}
			
			ht [string.Empty] = DefaultNamespace;
			ht ["xml"] = XmlnsXml;
			ht ["xmlns"] = XmlnsXmlns;
			
			return ht.Keys.GetEnumerator ();
		}

		public virtual bool HasNamespace (string prefix)
		{
			if (prefix == null || count == 0)
				return false;

			for (int i = declPos; i > declPos - count; i--) {
				if (decls [i].Prefix == prefix)
					return true;
			}
			
			return false;
		}

		public virtual string LookupNamespace (string prefix)
		{
			switch (prefix) {
			case "xmlns":
				return nameTable.Get (XmlnsXmlns);
			case "xml":
				return nameTable.Get (XmlnsXml);
			case "":
				return DefaultNamespace;
			}
			
			for (int i = declPos; i >= 0; i--) {
				if (decls [i].Prefix == prefix && decls [i].Uri != null /* null == flag for removed */)
					return decls [i].Uri;
			}
			
			return null;
		}

		public virtual string LookupPrefix (string uri)
		{
			if (uri == null)
				return null;
			
			if (uri == DefaultNamespace)
				return string.Empty;
				
			if (uri == XmlnsXml)
				return nameTable.Add ("xml");
			
			if (uri == XmlnsXmlns)
				return nameTable.Add ("xmlns");
			
			
			for (int i = declPos; i >= 0; i--) {
				if (decls [i].Uri == uri && decls [i].Prefix != string.Empty) // we already looked for ""
					return decls [i].Prefix;
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
			if (scopePos == -1)
				return false;

			declPos -= count;
			defaultNamespace = scopes [scopePos].DefaultNamespace;
			count = scopes [scopePos].DeclCount;
			scopePos --;
			return true;
		}

		public virtual void PushScope ()
		{
			scopePos ++;
			if (scopePos == scopes.Length)
				GrowScopes ();
			
			scopes [scopePos].DefaultNamespace = defaultNamespace;
			scopes [scopePos].DeclCount = count;
			count = 0;
		}

		public virtual void RemoveNamespace (string prefix, string uri)
		{
			if (prefix == null)
				throw new ArgumentNullException ("prefix");

			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			if (count == 0)
				return;		

			for (int i = declPos; i > declPos - count; i--) {
				if (decls [i].Prefix == prefix && decls [i].Uri == uri)
					decls [i].Uri = null;
			}
		}

		#endregion
	}
}
