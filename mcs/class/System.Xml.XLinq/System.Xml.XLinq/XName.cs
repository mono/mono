#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using XPI = System.Xml.XLinq.XProcessingInstruction;

namespace System.Xml.XLinq
{
	public class XName
	{
		string expanded;
		string local;
		string ns;

		private XName (string expanded)
		{
			if (expanded == null || expanded.Length == 0)
				throw ErrorInvalidExpandedName ();
			this.expanded = expanded;
			if (expanded [0] == '{') {
				for (int i = 1; i < expanded.Length; i++) {
					if (expanded [i] == '}')
						ns = expanded.Substring (1, i - 1);
				}
				if (ns == null || ns.Length == 0) // {}foo is invalid
					throw ErrorInvalidExpandedName ();
				if (expanded.Length == ns.Length + 2) // {foo} is invalid
					throw ErrorInvalidExpandedName ();
				local = expanded.Substring (ns.Length + 2);
			}
			else {
				local = expanded;
				ns = String.Empty;
			}
		}

		private XName (string local, string ns)
		{
			this.local = local;
			this.ns = ns;
		}

		private Exception ErrorInvalidExpandedName ()
		{
			return new ArgumentException ("Invalid expanded name.");
		}

		public string ExpandedName {
			get {
				if (expanded == null)
					expanded = ns != null ? String.Concat ("{", ns, "}", local) : local;
				return expanded;
			}
		}

		public string LocalName {
			get { return local; }
		}

		public string NamespaceName {
			get { return ns; }
		}

		public override bool Equals (object obj)
		{
			XName n = obj as XName;
			return n != null && this == n;
		}

		public static XName Get (string expandedName)
		{
			return new XName (expandedName);
		}

		public static XName Get (string localName, string namespaceName)
		{
			return new XName (localName, namespaceName);
		}

		public override int GetHashCode ()
		{
			return local.GetHashCode () ^ ns.GetHashCode ();
		}

		public static bool operator == (XName n1, XName n2)
		{
			if ((object) n1 == null)
				return (object) n2 == null;
			else if ((object) n2 == null)
				return false;
			return object.ReferenceEquals (n1, n2) ||
				n1.local == n2.local && n1.ns == n2.ns;
		}

		public static implicit operator XName (string s)
		{
			return s == null ? null : Get (s);
		}

		public static bool operator != (XName n1, XName n2)
		{
			return ! (n1 == n2);
		}

		public override string ToString ()
		{
			return ExpandedName;
		}
	}
}

#endif
