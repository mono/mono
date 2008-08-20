//
// PolicyAssertionCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;

namespace System.ServiceModel.Description
{
	public class PolicyAssertionCollection
		: Collection<XmlElement>
	{
		public PolicyAssertionCollection ()
		{
		}

		public PolicyAssertionCollection (IEnumerable<XmlElement> list)
			: base (new List<XmlElement> (list))
		{
		}

		[MonoTODO]
		public bool Contains (string name, string ns)
		{
			foreach (XmlElement el in this)
				if (el.LocalName == name && el.NamespaceURI == ns)
					return true;
			return false;
		}

		[MonoTODO]
		public XmlElement Find (string name, string ns)
		{
			foreach (XmlElement el in this)
				if (el.LocalName == name && el.NamespaceURI == ns)
					return el;
			return null;
		}

		[MonoTODO]
		public Collection<XmlElement> FindAll (string name, string ns)
		{
			Collection<XmlElement> ret =
				new Collection<XmlElement> ();
			foreach (XmlElement el in this)
				if (el.LocalName == name && el.NamespaceURI == ns)
					ret.Add (el);
			return ret;
		}

		[MonoTODO]
		public XmlElement Remove (string name, string ns)
		{
			foreach (XmlElement el in this)
				if (el.LocalName == name && el.NamespaceURI == ns) {
					Remove (el);
					return el;
				}
			return null;
		}

		[MonoTODO]
		public Collection<XmlElement> RemoveAll (string name, string ns)
		{
			Collection<XmlElement> list = FindAll (name, ns);
			foreach (XmlElement el in list)
				Remove (el);
			return list;
		}

		[MonoTODO]
		protected override void InsertItem (int index, XmlElement item)
		{
			base.InsertItem (index, item);
		}

		[MonoTODO]
		protected override void SetItem (int index, XmlElement item)
		{
			base.SetItem (index, item);
		}
	}
}
