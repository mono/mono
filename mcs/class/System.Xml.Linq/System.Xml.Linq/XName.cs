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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

using XPI = System.Xml.Linq.XProcessingInstruction;

namespace System.Xml.Linq
{
	[Serializable]
	public sealed class XName : IEquatable<XName>, ISerializable
	{
		string local;
		XNamespace ns;

		XName (SerializationInfo info, StreamingContext context)
		{
			string expandedName = info.GetString ("name");
			string local, ns;
			ExpandName (expandedName, out local, out ns);
			this.local = local;
			this.ns = XNamespace.Get (ns);
		}

		internal XName (string local, XNamespace ns)
		{
			this.local = XmlConvert.VerifyNCName (local);
			this.ns = ns;
		}

		static Exception ErrorInvalidExpandedName ()
		{
			return new ArgumentException ("Invalid expanded name.");
		}

		public string LocalName {
			get { return local; }
		}

		public XNamespace Namespace {
			get { return ns; }
		}

		public string NamespaceName {
			get { return ns.NamespaceName; }
		}

		public override bool Equals (object obj)
		{
			XName n = obj as XName;
			return n != null && this == n;
		}

		bool IEquatable<XName>.Equals (XName other)
		{
			return this == other;
		}

		public static XName Get (string expandedName)
		{
			string local, ns;
			ExpandName (expandedName, out local, out ns);
			return Get (local, ns);
		}

		static void ExpandName (string expandedName, out string local, out string ns)
		{
			if (expandedName == null)
				throw new ArgumentNullException ("expandedName");
			ns = null;
			local = null;
			if (expandedName.Length == 0)
				throw ErrorInvalidExpandedName ();
			//this.expanded = expandedName;
			if (expandedName [0] == '{') {
				for (int i = 1; i < expandedName.Length; i++) {
					if (expandedName [i] == '}')
						ns = expandedName.Substring (1, i - 1);
				}
				if (String.IsNullOrEmpty (ns)) // {}foo is invalid
					throw ErrorInvalidExpandedName ();
				if (expandedName.Length == ns.Length + 2) // {foo} is invalid
					throw ErrorInvalidExpandedName ();
				local = expandedName.Substring (ns.Length + 2);
			}
			else {
				local = expandedName;
				ns = String.Empty;
			}
		}

		public static XName Get (string localName, string namespaceName)
		{
			return XNamespace.Get (namespaceName).GetName (localName);
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
			if (ns == XNamespace.None)
				return local;
			return String.Concat ("{", ns.NamespaceName, "}", local);
		}

		// in .NET it is serialized as "NameSerializer". dunno how to create it.
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("name", ToString ());
		}
	}
}

#endif
