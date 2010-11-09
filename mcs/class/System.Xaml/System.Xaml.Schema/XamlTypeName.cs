//
// Copyright (C) 2010 Novell Inc. http://novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Xaml.Schema
{
	public class XamlTypeName
	{
		public static XamlTypeName Parse (string typeName, IXamlNamespaceResolver namespaceResolver)
		{
			XamlTypeName n;
			if (!TryParse (typeName, namespaceResolver, out n))
				throw new FormatException (String.Format ("Invalid typeName: '{0}'", typeName));
			return n;
		}

		public static bool TryParse (string typeName, IXamlNamespaceResolver namespaceResolver, out XamlTypeName result)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");
			if (namespaceResolver == null)
				throw new ArgumentNullException ("namespaceResolver");

			result = null;
			IList<XamlTypeName> args = null;

			int idx = typeName.IndexOf ('(');
			if (idx >= 0) {
				if (typeName [typeName.Length - 1] != ')')
					return false;
				if (!TryParseList (typeName.Substring (idx + 1, typeName.Length - idx - 2), namespaceResolver, out args))
					return false;
				typeName = typeName.Substring (0, idx);
			}

			idx = typeName.IndexOf (':');
			string prefix, local;
			if (idx < 0) {
				prefix = String.Empty;
				local = typeName;
			} else {
				prefix = typeName.Substring (0, idx);
				local = typeName.Substring (idx + 1);
				if (!XamlLanguage.IsValidXamlName (prefix))
					return false;
			}
			if (!XamlLanguage.IsValidXamlName (local))
				return false;
			string ns = namespaceResolver.GetNamespace (prefix);
			if (ns == null)
				return false;

			result = new XamlTypeName (ns, local, args);
			return true;
		}

		static readonly char [] commas = {','};

		public static IList<XamlTypeName> ParseList (string typeNameList, IXamlNamespaceResolver namespaceResolver)
		{
			IList<XamlTypeName> list;
			if (!TryParseList (typeNameList, namespaceResolver, out list))
				throw new FormatException (String.Format ("Invalid type name list: '{0}'", typeNameList));
			return list;
		}

		public static bool TryParseList (string typeNameList, IXamlNamespaceResolver namespaceResolver, out IList<XamlTypeName> list)
		{
			if (typeNameList == null)
				throw new ArgumentNullException ("typeNameList");
			if (namespaceResolver == null)
				throw new ArgumentNullException ("namespaceResolver");

			list = null;
			var split = typeNameList.Split (commas);
			if (split.Length == 0)
				return false;

			var arr = new XamlTypeName [split.Length];

			for (int i = 0; i < split.Length; i++) {
				var s = split [i].Trim ();
				XamlTypeName tn;
				if (!TryParse (s, namespaceResolver, out tn))
					return false;
				arr [i] = tn;
			}

			list = arr;
			return true;
		}

		public static string ToString (IList<XamlTypeName> typeNameList, INamespacePrefixLookup prefixLookup)
		{
			if (typeNameList == null)
				throw new ArgumentNullException ("typeNameList");
			if (prefixLookup == null)
				throw new ArgumentNullException ("prefixLookup");

			return DoToString (typeNameList, prefixLookup);
		}

		static string DoToString (IList<XamlTypeName> typeNameList, INamespacePrefixLookup prefixLookup)
		{
			bool comma = false;
			string ret = "";
			foreach (var ta in typeNameList) {
				if (comma)
					ret += ", ";
				else
					comma = true;
				ret += ta.ToString (prefixLookup);
			}
			return ret;
		}

		// instance members

		public XamlTypeName ()
		{
			TypeArguments = empty_type_args;
		}

		static readonly XamlTypeName [] empty_type_args = new XamlTypeName [0];

		public XamlTypeName (XamlType xamlType)
			: this ()
		{
			if (xamlType == null)
				throw new ArgumentNullException ("xamlType");
			Namespace = xamlType.PreferredXamlNamespace;
			Name = xamlType.Name;
			if (xamlType.TypeArguments != null && xamlType.TypeArguments.Count > 0) {
				var l = new List<XamlTypeName> ();
				l.AddRange (from x in xamlType.TypeArguments.AsQueryable () select new XamlTypeName (x));
				TypeArguments = l;
			}
		}
		
		public XamlTypeName (string xamlNamespace, string name)
			: this (xamlNamespace, name, null)
		{
		}

		public XamlTypeName (string xamlNamespace, string name, IEnumerable<XamlTypeName> typeArguments)
			: this ()
		{
			Namespace = xamlNamespace;
			Name = name;
			if (typeArguments != null) {
				if (typeArguments.Any (t => t == null))
					throw new ArgumentNullException ("typeArguments", "typeArguments array contains one or more null XamlTypeName");
				var l = new List<XamlTypeName> ();
				l.AddRange (typeArguments);
				TypeArguments = l;
			}
		}

		public string Name { get; set; }
		public string Namespace { get; set; }
		public IList<XamlTypeName> TypeArguments { get; private set; }

		public override string ToString ()
		{
			return ToString (null);
		}

		public string ToString (INamespacePrefixLookup prefixLookup)
		{
			if (Namespace == null)
				throw new InvalidOperationException ("Namespace must be set before calling ToString method.");
			if (Name == null)
				throw new InvalidOperationException ("Name must be set before calling ToString method.");

			string ret;
			if (prefixLookup == null)
				ret = String.Concat ("{", Namespace, "}", Name);
			else {
				string p = prefixLookup.LookupPrefix (Namespace);
				if (p == null)
					throw new InvalidOperationException (String.Format ("Could not lookup prefix for namespace '{0}'", Namespace));
				ret = p.Length == 0 ? Name : p + ":" + Name;
			}

			if (TypeArguments.Count > 0)
				ret += String.Concat ("(", DoToString (TypeArguments, prefixLookup), ")");

			return ret;
		}
	}
}
