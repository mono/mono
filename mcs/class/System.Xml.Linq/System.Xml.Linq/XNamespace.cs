//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Xml.Linq
{
	public sealed class XNamespace
	{
		static readonly XNamespace blank, xml, xmlns;
		static Dictionary<string, XNamespace> nstable;

		static XNamespace ()
		{
			nstable = new Dictionary<string, XNamespace> ();
			blank = Get (String.Empty);
			xml = Get ("http://www.w3.org/XML/1998/namespace");
			xmlns = Get ("http://www.w3.org/2000/xmlns/");
		}

		public static XNamespace None { 
			get { return blank; }
		}

		public static XNamespace Xml {
			get { return xml; }
		}

		public static XNamespace Xmlns {
			get { return xmlns; }
		}

		public static XNamespace Get (string namespaceName)
		{
			lock (nstable) {
				XNamespace ret;
				if (!nstable.TryGetValue (namespaceName, out ret)) {
					ret = new XNamespace (namespaceName);
					nstable [namespaceName] = ret;
				}
				return ret;
			}
		}

		public XName GetName (string localName)
		{
			if (table == null)
				table = new Dictionary<string, XName> ();
			lock (table) {
				XName ret;
				if (!table.TryGetValue (localName, out ret)) {
					ret = new XName (localName, this);
					table [localName] = ret;
				}
				return ret;
			}
		}

		string uri;
		Dictionary<string, XName> table;

		XNamespace (string namespaceName)
		{
			if (namespaceName == null)
				throw new ArgumentNullException ("namespaceName");
			uri = namespaceName;
		}

		public string NamespaceName {
			get { return uri; }
		}

		public override bool Equals (object obj)
		{
			if (Object.ReferenceEquals (this, obj))
				return true;
			XNamespace ns = obj as XNamespace;
			return ns != null && uri == ns.uri;
		}

		public static bool operator == (XNamespace left, XNamespace right)
		{
			return (object) left != null ? left.Equals (right) : (object) right == null;
		}

		public static bool operator != (XNamespace left, XNamespace right)
		{
			return ! (left == right);
		}
		
		public static XName operator + (XNamespace ns, string localName)
		{
			return new XName (localName, ns);
		}

		[CLSCompliant (false)]
		public static implicit operator XNamespace (string namespaceName)
		{
			return namespaceName != null ? XNamespace.Get (namespaceName) : null;
		}

		public override int GetHashCode ()
		{
			return uri.GetHashCode ();
		}

		public override string ToString ()
		{
			return uri;
		}
	}
}
