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
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace System.Xml.Linq
{
	public abstract class XObject : IXmlLineInfo
	{
		internal XObject ()
		{
		}

		XContainer owner;
		List<object> annotations;
		string baseuri;
		int line, column;

		public event EventHandler<XObjectChangeEventArgs> Changing;
		public event EventHandler<XObjectChangeEventArgs> Changed;

		public string BaseUri {
			get { return baseuri; }
			internal set { baseuri = value; }
		}

		public XDocument Document {
			get {
				if (this is XDocument)
					return (XDocument) this;

				for (XContainer e = owner; e != null; e = e.owner)
					if (e is XDocument)
						return (XDocument) e;
				return null;
			}
		}

		public abstract XmlNodeType NodeType { get; }

		public XElement Parent {
			get { return owner as XElement; }
		}

		internal XContainer Owner {
			get { return owner; }
		}

		internal void SetOwner (XContainer node)
		{
			owner = node;
		}

		public void AddAnnotation (object annotation)
		{
			if (annotation == null)
				throw new ArgumentNullException ("annotation");
			if (annotations == null)
				annotations = new List<object> ();
			annotations.Add (annotation);
		}

		public T Annotation<T> () where T : class
		{
			return (T) Annotation (typeof (T));
		}

		public object Annotation (Type type)
		{
			return Annotations (type).FirstOrDefault ();
		}

		public IEnumerable<T> Annotations<T> () where T : class
		{
			foreach (T o in Annotations (typeof (T)))
				yield return o;
		}

		public IEnumerable<object> Annotations (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (annotations == null)
				yield break;
			foreach (object o in annotations)
				if (type.IsAssignableFrom (o.GetType ()))
					yield return o;
		}

		public void RemoveAnnotations<T> () where T : class
		{
			RemoveAnnotations (typeof (T));
		}

		public void RemoveAnnotations (Type type)
		{
			if (annotations == null)
				return;
			for (int i = 0; i < annotations.Count; i++)
				if (type.IsAssignableFrom (annotations [i].GetType ()))
					annotations.RemoveAt (i);
		}

		internal int LineNumber {
			get { return line; }
			set { line = value; }
		}

		internal int LinePosition {
			get { return column; }
			set { column = value; }
		}

		int IXmlLineInfo.LineNumber {
			get { return LineNumber; }
		}

		int IXmlLineInfo.LinePosition {
			get { return LinePosition; }
		}

		bool IXmlLineInfo.HasLineInfo ()
		{
			return line > 0;
		}

		internal void FillLineInfoAndBaseUri (XmlReader r, LoadOptions options)
		{
			if ((options & LoadOptions.SetLineInfo) != LoadOptions.None) {
				IXmlLineInfo li = r as IXmlLineInfo;
				if (li != null && li.HasLineInfo ()) {
					LineNumber = li.LineNumber;
					LinePosition = li.LinePosition;
				}
			}
			if ((options & LoadOptions.SetBaseUri) != LoadOptions.None)
				BaseUri = r.BaseURI;
		}
		
		internal virtual void OnAddingObject ()
		{
			if (Parent != null)
				Parent.OnAddingObject ();
			if (Changing != null)
				Changing.Invoke (this, null);
		}
		
		internal virtual void OnAddedObject ()
		{
			if (Parent != null)
				Parent.OnAddedObject ();
			if (Changed != null)
				Changed.Invoke (this, null);
		}
	}
}
