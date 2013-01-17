using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace Mono.Documentation {

	public delegate XmlDocument DocLoader (string escapedTypeName);

	public static class XmlDocUtils
	{
		public static XmlNodeList GetMemberGenericParameters (XmlNode member)
		{
			return member.SelectNodes ("Docs/typeparam");
		}

		public static XmlNodeList GetTypeGenericParameters (XmlNode member)
		{
			return member.SelectNodes ("/Type/TypeParameters/TypeParameter");
		}

		public static string ToTypeName (string type, XmlNode member)
		{
			return ToTypeName (type, GetTypeGenericParameters (member), 
					GetMemberGenericParameters (member));
		}

		public static string ToTypeName (string type, XmlNodeList typeGenParams, XmlNodeList memberGenParams)
		{
			type = type.Replace ("&", "@").Replace ("<", "{").Replace (">", "}");
			for (int i = 0; i < typeGenParams.Count; ++i) {
				string name = typeGenParams [i].InnerText;
				type = Regex.Replace (type, @"\b" + name + @"\b", "`" + i);
			}
			for (int i = 0; i < memberGenParams.Count; ++i) {
				string name = memberGenParams [i].Attributes ["name"].Value;
				type = Regex.Replace (type, @"\b" + name + @"\b", "``" + i);
			}
			return type;
		}

		public static string ToEscapedTypeName (string name)
		{
			return GetCountedName (name, "`");
		}

		private static string GetCountedName (string name, string escape)
		{
			int lt = name.IndexOf ("<");
			if (lt == -1)
				return name;
			StringBuilder type = new StringBuilder (name.Length);
			int start = 0;
			do {
				type.Append (name.Substring (start, lt - start));
				type.Append (escape);
				type.Append (GetGenericCount (name, lt, out start));
			} while ((lt = name.IndexOf ('<', start)) >= 0);
			if (start < name.Length)
				type.Append (name.Substring (start));
			return type.ToString ().Replace ("+", ".");
		}

		private static int GetGenericCount (string name, int start, out int end)
		{
			int n = 1;
			bool r = true;
			int i = start;
			int depth = 1;
			for ( ++i; r && i < name.Length; ++i) {
				switch (name [i]) {
					case ',': if (depth == 1) ++n; break;
					case '<': ++depth; break;
					case '>': --depth; if (depth == 0) r = false; break;
				}
			}
			end = i;
			return n;
		}

		public static string ToEscapedMemberName (string member)
		{
			// Explicitly implemented interface members contain '.'s in the member
			// name, e.g. System.Collections.Generic.IEnumerable<A>.GetEnumerator.
			// CSC does a s/\./#/g for these.
			member = member.Replace (".", "#");
			if (member [member.Length-1] == '>') {
				int i = member.LastIndexOf ("<");
				int ignore;
				return member.Substring (0, i).Replace ("<", "{").Replace (">", "}") + 
					"``" + GetGenericCount (member, i, out ignore);
			}
			return member.Replace ("<", "{").Replace (">", "}");
		}

		public static void AddExtensionMethods (XmlDocument typexml, ArrayList/*<XmlNode>*/ extensions, DocLoader loader)
		{
			// if no members (enum, delegate) don't add extensions
			XmlNode m = typexml.SelectSingleNode ("/Type/Members");
			if (m == null)
				return;

			// static classes can't be targets:
			if (typexml.SelectSingleNode (
						"/Type/TypeSignature[@Language='C#']/@Value")
					.Value.IndexOf (" static ") >= 0)
				return;

			foreach (string s in GetSupportedTypes (typexml, loader)) {
				foreach (XmlNode extension in extensions) {
					bool add = false;
					foreach (XmlNode target in extension.SelectNodes ("Targets/Target")) {
						if (target.Attributes ["Type"].Value == s) {
							add = true;
							break;
						}
					}
					if (!add) {
						continue;
					}
					foreach (XmlNode c in extension.SelectNodes ("Member")) {
						XmlNode cm = typexml.ImportNode (c, true);
						m.AppendChild (cm);
					}
				}
			}
		}

		private static IEnumerable GetSupportedTypes (XmlDocument type, DocLoader loader)
		{
			yield return "System.Object";
			yield return GetEscapedPath (type, "Type/@FullName");

			Hashtable h = new Hashtable ();
			GetInterfaces (h, type, loader);

			string s = GetEscapedPath (type, "Type/Base/BaseTypeName");
			if (s != null) {
				yield return s;
				XmlDocument d;
				string p = s;
				while (s != null && (d = loader (s)) != null) {
					GetInterfaces (h, d, loader);
					s = GetEscapedPath (d, "Type/Base/BaseTypeName");
					if (p == s)
						break;
					yield return s;
				}
			}

			foreach (object o in h.Keys)
				yield return o.ToString ();
		}

		private static string GetEscapedPath (XmlDocument d, string path)
		{
			XmlNode n = d.SelectSingleNode (path);
			if (n == null)
				return null;
			return "T:" + ToEscapedTypeName (n.InnerText);
		}

		private static void GetInterfaces (Hashtable ifaces, XmlDocument doc, DocLoader loader)
		{
			foreach (XmlNode n in doc.SelectNodes ("Type/Interfaces/Interface/InterfaceName")) {
				string t = ToEscapedTypeName (n.InnerText);
				string tk = "T:" + t;
				if (!ifaces.ContainsKey (tk)) {
					ifaces.Add (tk, null);
					try {
						XmlDocument d = loader (t);
						if (d != null)
							GetInterfaces (ifaces, d, loader);
					}
					catch (FileNotFoundException e) {
						// ignore; interface documentation couldn't be found.
					}
				}
			}
		}

		// Turns e.g. sources/netdocs into sources/cache/netdocs
		public static string GetCacheDirectory (string assembledBase)
		{
			return Path.Combine (
						Path.Combine (Path.GetDirectoryName (assembledBase), "cache"),
						Path.GetFileName (assembledBase));
		}

		public static string GetCachedFileName (string cacheDir, string url)
		{
			return Path.Combine (cacheDir,
			                     Uri.EscapeUriString (url).Replace ('/', '+').Replace ("*", "%2a"));
		}
	}
}

