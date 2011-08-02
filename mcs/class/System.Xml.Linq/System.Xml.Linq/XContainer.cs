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
	public abstract class XContainer : XNode
	{
		internal XContainer ()
		{
		}

		XNode first;
		XNode last;

		public XNode FirstNode {
			get { return first; }
			internal set { first = value; }
		}

		public XNode LastNode {
			get { return last; }
			internal set { last = value; }
		}

		void CheckChildType (object o, bool addFirst)
		{
			if (o == null || o is string || o is XNode)
				return;
			if (o is IEnumerable) {
				foreach (object oc in ((IEnumerable) o))
					CheckChildType (oc, addFirst);
				return;
			}
			else
				throw new ArgumentException ("Invalid child type: " + o.GetType ());
		}

		public void Add (object content)
		{
			if (content == null)
				return;

			foreach (object o in XUtil.ExpandArray (content))
			{
				if (!OnAddingObject (o, false, last, false))
				{
					OnAddingObject ();
					AddNode (XUtil.ToNode (o));
					OnAddedObject ();
				}
			}
		}
		
		void AddNode (XNode n)
		{
			CheckChildType (n, false);
			n = (XNode) XUtil.GetDetachedObject (n);
			n.SetOwner (this);
			if (first == null)
				last = first = n;
			else {
				last.NextNode = n;
				n.PreviousNode = last;
				last = n;
			}
		}

		public void Add (params object [] content)
		{
			if (content == null)
				return;
			foreach (object o in XUtil.ExpandArray (content))
				Add (o);
		}

		public void AddFirst (object content)
		{
			if (first == null)
				Add (content);
			else
				first.AddBeforeSelf (XUtil.ExpandArray (content));
		}

		public void AddFirst (params object [] content)
		{
			if (content == null)
				return;
			if (first == null)
				Add (content);
			else
				foreach (object o in XUtil.ExpandArray (content))
					if (!OnAddingObject (o, false, first.PreviousNode, true))
						first.AddBeforeSelf (o);
		}

		internal virtual bool OnAddingObject (object o, bool rejectAttribute, XNode refNode, bool addFirst)
		{
			return false;
		}
		
		public XmlWriter CreateWriter ()
		{
			return new XNodeWriter (this);
		}

		public IEnumerable <XNode> Nodes ()
		{
			XNode next;
			for (XNode n = FirstNode; n != null; n = next) {
				next = n.NextNode;
				yield return n;
			}
		}

		public IEnumerable<XNode> DescendantNodes ()
		{
			foreach (XNode n in Nodes ()) {
				yield return n;
				XContainer c = n as XContainer;
				if (c != null)
					foreach (XNode d in c.DescendantNodes ())
						yield return d;
			}
		}

		public IEnumerable <XElement> Descendants ()
		{
			foreach (XNode n in DescendantNodes ()) {
				XElement el = n as XElement;
				if (el != null)
					yield return el;
			}
		}

		public IEnumerable <XElement> Descendants (XName name)
		{
			foreach (XElement el in Descendants ())
				if (el.Name == name)
					yield return el;
		}

		public IEnumerable <XElement> Elements ()
		{
			foreach (XNode n in Nodes ()) {
				XElement el = n as XElement;
				if (el != null)
					yield return el;
			}
		}

		public IEnumerable <XElement> Elements (XName name)
		{
			foreach (XElement el in Elements ())
				if (el.Name == name)
					yield return el;
		}

		public XElement Element (XName name)
		{
			foreach (XElement el in Elements ())
				if (el.Name == name)
					return el;
			return null;
		}

		internal void ReadContentFrom (XmlReader reader, LoadOptions options)
		{
			while (!reader.EOF) {
				if (reader.NodeType == XmlNodeType.EndElement)
					// end of the element.
					break;
				Add (XNode.ReadFrom (reader, options));
			}
		}

		public void RemoveNodes ()
		{
			foreach (XNode n in Nodes ())
				n.Remove ();
		}

		public void ReplaceNodes (object content)
		{
			// First, it creates a snapshot copy, then removes the contents, and then adds the copy. http://msdn.microsoft.com/en-us/library/system.xml.linq.xcontainer.replacenodes.aspx

			if (FirstNode == null) {
				Add (content);
				return;
			}

			var l = new List<object> ();
			foreach (var obj in XUtil.ExpandArray (content))
				l.Add (obj);

			RemoveNodes ();
			Add (l);
		}

		public void ReplaceNodes (params object [] content)
		{
			ReplaceNodes ((object) content);
		}
	}
}
