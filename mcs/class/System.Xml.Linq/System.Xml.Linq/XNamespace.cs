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
		static readonly XNamespace blank = Get (String.Empty);
		static readonly XNamespace xml = Get ("http://www.w3.org/XML/1998/namespace");
		static readonly XNamespace xmlns = Get ("http://www.w3.org/2000/xmlns/");

		internal static XNamespace Blank {
			get { return blank; }
		}

		public static XNamespace Xml {
			get { return xml; }
		}

		public static XNamespace Xmlns {
			get { return xmlns; }
		}

		[MonoTODO]
		public static XNamespace Get (string uri)
		{
			return new XNamespace (uri);
		}

		[MonoTODO]
		public XName GetName (string localName)
		{
			return new XName (localName, this);
		}

		string uri;

		XNamespace (string namespaceName)
		{
			if (namespaceName == null)
				throw new ArgumentNullException ("namespaceName");
			uri = namespaceName;
		}
		
		[MonoTODO]
		public static XNamespace None { 
			get {
				return null;
			}
		}

		public string NamespaceName {
			get { return uri; }
		}

		public override bool Equals (object other)
		{
			XNamespace ns = other as XNamespace;
			return ns != null && uri == ns.uri;
		}

		public static bool operator == (XNamespace o1, XNamespace o2)
		{
			return (object) o1 != null ? o1.Equals (o2) : (object) o2 == null;
		}

		public static bool operator != (XNamespace o1, XNamespace o2)
		{
			return ! (o1 == o2);
		}
		
		public static XName operator +(XNamespace ns, string localName)
		{
			return null;
		}

		public static implicit operator XNamespace (string s)
		{
			return s != null ? XNamespace.Get (s) : null;
		}

		public override int GetHashCode ()
		{
			return uri.GetHashCode ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			return uri.ToString ();
		}
	}
}
