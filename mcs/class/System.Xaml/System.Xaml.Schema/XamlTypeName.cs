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
		public static bool TryParse (string typeName, IXamlNamespaceResolver namespaceResolver, out XamlTypeName result)
		{
			throw new NotImplementedException ();
		}

		public static IList<XamlTypeName> ParseList (string typeNameList, IXamlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		public static string ToString (IList<XamlTypeName> typeNameList, INamespacePrefixLookup prefixLookup)
		{
			throw new NotImplementedException ();
		}

		// instance members

		public XamlTypeName ()
		{
		}
		
		public XamlTypeName (XamlType xamlType)
		{
			Namespace = xamlType.PreferredXamlNamespace;
			Name = xamlType.Name;
			var l = new List<XamlTypeName> ();
			l.AddRange (from x in xamlType.TypeArguments.AsQueryable () select new XamlTypeName (x));
			TypeArguments = l;
		}
		
		public XamlTypeName (string xamlNamespace, string name)
			: this (xamlNamespace, name, null)
		{
		}

		public XamlTypeName (string xamlNamespace, string name, IEnumerable<XamlTypeName> typeArguments)
		{
			Namespace = xamlNamespace;
			Name = name;
			if (typeArguments != null) {
				var l = new List<XamlTypeName> ();
				l.AddRange (typeArguments);
				TypeArguments = l.Count > 0 ? l : null;
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
			throw new NotImplementedException ();
		}
	}
}
