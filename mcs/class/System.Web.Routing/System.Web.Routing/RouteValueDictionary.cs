//
// RouteValueDictionary.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
//

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
using System.Security.Permissions;
using System.Web;

namespace System.Web.Routing
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class RouteValueDictionary : IDictionary<string, object>, 
		ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, 
		IEnumerable
	{
		[MonoTODO]
		public RouteValueDictionary ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RouteValueDictionary (IDictionary<string, object> dictionary)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RouteValueDictionary (object values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Count { get; private set; }
		[MonoTODO]
		bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		ICollection<string> IDictionary<string, object>.Keys {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		ICollection<Object> IDictionary<string, object>.Values {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object this [string key] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Dictionary<string, object>.KeyCollection Keys { get; private set; }

		[MonoTODO]
		public Dictionary<string, object>.ValueCollection Values { get; private set; }

		[MonoTODO]
		public void Add (string key, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool ContainsKey (string key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool ContainsValue (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Dictionary<string, object>.Enumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ICollection<KeyValuePair<string, object>>.Add (KeyValuePair<string, object> item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool ICollection<KeyValuePair<string, object>>.Contains (KeyValuePair<string, object> item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ICollection<KeyValuePair<string, object>>.CopyTo (KeyValuePair<string, object> [] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool ICollection<KeyValuePair<string, object>>.Remove (KeyValuePair<string, object> item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Remove (string key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool TryGetValue (string key, out object value)
		{
			throw new NotImplementedException ();
		}
	}
}
