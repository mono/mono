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

using XPI = System.Xml.Linq.XProcessingInstruction;

namespace System.Xml.Linq
{
	public sealed class XNodeEqualityComparer : IEqualityComparer, IEqualityComparer<XNode>
	{
		public XNodeEqualityComparer ()
		{
		}

		public bool Equals (XNode x, XNode y)
		{
			if (x == null)
				return y == null;
			else if (y == null)
				return false;
			//throw new NotImplementedException ();
			if (x.NodeType != y.NodeType)
				return false;
			switch (x.NodeType) {
			case XmlNodeType.Document:
				XDocument doc1 = (XDocument) x;
				XDocument doc2 = (XDocument) y;
				if (!Equals (doc1.Declaration, doc2.Declaration))
					return false;
				IEnumerator<XNode> id2 = doc2.Nodes ().GetEnumerator ();
				foreach (XNode n in doc1.Nodes ()) {
					if (!id2.MoveNext ())
						return false;
					if (!Equals (n, id2.Current))
						return false;
				}
				return !id2.MoveNext ();
			case XmlNodeType.Element:
				XElement e1 = (XElement) x;
				XElement e2 = (XElement) y;
				if (e1.Name != e2.Name)
					return false;
				IEnumerator<XAttribute> ia2 = e2.Attributes ().GetEnumerator ();
				foreach (XAttribute n in e1.Attributes ()) {
					if (!ia2.MoveNext ())
						return false;
					if (!Equals (n, ia2.Current))
						return false;
				}
				if (ia2.MoveNext ())
					return false;
				IEnumerator<XNode> ie2 = e2.Nodes ().GetEnumerator ();
				foreach (XNode n in e1.Nodes ()) {
					if (!ie2.MoveNext ())
						return false;
					if (!Equals (n, ie2.Current))
						return false;
				}
				return !ie2.MoveNext ();
			case XmlNodeType.Comment:
				XComment c1 = (XComment) x;
				XComment c2 = (XComment) y;
				return c1.Value == c2.Value;
			case XmlNodeType.ProcessingInstruction:
				XPI p1 = (XPI) x;
				XPI p2 = (XPI) y;
				return p1.Target == p2.Target && p1.Data == p2.Data;
			case XmlNodeType.DocumentType:
				XDocumentType d1 = (XDocumentType) x;
				XDocumentType d2 = (XDocumentType) y;
				return d1.Name == d2.Name &&
				       d1.PublicId == d2.PublicId &&
				       d1.SystemId == d2.SystemId &&
				       d1.InternalSubset == d2.InternalSubset;
			case XmlNodeType.Text:
				return ((XText) x).Value == ((XText) y).Value;
			}
			throw new Exception ("INTERNAL ERROR: should not happen");
		}

		bool Equals (XAttribute a1, XAttribute a2)
		{
			if (a1 == null)
				return a2 == null;
			else if (a2 == null)
				return false;
			return a1.Name == a2.Name && a1.Value == a2.Value;
		}

		bool Equals (XDeclaration d1, XDeclaration d2)
		{
			if (d1 == null)
				return d2 == null;
			else if (d2 == null)
				return false;
			return d1.Version == d2.Version &&
			       d1.Encoding == d2.Encoding &&
			       d1.Standalone == d2.Standalone;
		}

		bool IEqualityComparer.Equals (object n1, object n2)
		{
			return Equals ((XNode) n1, (XNode) n2);
		}

		int GetHashCode (XDeclaration d)
		{
			if (d == null)
				return 0;
			return (d.Version.GetHashCode () << 7) ^
			       (d.Encoding.GetHashCode () << 6) ^
			       d.Standalone.GetHashCode ();
		}

		public int GetHashCode (XNode obj)
		{
			if (obj == null)
				return 0;
			int h = ((int) obj.NodeType << 6);
			switch (obj.NodeType) {
			case XmlNodeType.Document:
				XDocument doc = (XDocument) obj;
				h = h ^ GetHashCode (doc.Declaration);
				foreach (XNode n in doc.Nodes ())
					h = h ^ (n.GetHashCode () << 5);
				break;
			case XmlNodeType.Element:
				XElement el = (XElement) obj;
				h = h ^ (el.Name.GetHashCode () << 3);
				foreach (XAttribute a in el.Attributes ())
					h = h ^ (a.GetHashCode () << 7);
				foreach (XNode n in el.Nodes ())
					h = h ^ (n.GetHashCode () << 6);
				break;
			case XmlNodeType.Comment:
				h = h ^ ((XComment) obj).Value.GetHashCode ();
				break;
			case XmlNodeType.ProcessingInstruction:
				XPI pi = (XPI) obj;
				h = h ^ ((pi.Target.GetHashCode () << 6) + pi.Data.GetHashCode ());
				break;
			case XmlNodeType.DocumentType:
				XDocumentType dtd = (XDocumentType) obj;
				h = h ^ (dtd.Name.GetHashCode () << 7) ^
				    (dtd.PublicId.GetHashCode () << 6) ^
				    (dtd.SystemId.GetHashCode () << 5) ^
				    (dtd.InternalSubset.GetHashCode () << 4);
				break;
			case XmlNodeType.Text:
				h = h ^ (((XText) obj).GetHashCode ());
				break;
			}
			return h;
		}

		int IEqualityComparer.GetHashCode (object obj)
		{
			return GetHashCode ((XNode) obj);
		}
	}
}
