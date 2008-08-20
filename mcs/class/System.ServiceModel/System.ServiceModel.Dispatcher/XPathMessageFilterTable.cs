//
// XPathMessageFilterTable.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Xml.XPath;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	[DataContract]
	public class XPathMessageFilterTable<TFilterData>
		: IDictionary<MessageFilter,TFilterData>,
		  IMessageFilterTable<TFilterData>,
		  IEnumerable,
		  ICollection<KeyValuePair<MessageFilter,TFilterData>>,
		  IEnumerable<KeyValuePair<MessageFilter,TFilterData>>
	{
		Dictionary<MessageFilter,TFilterData> dict
			= new Dictionary<MessageFilter,TFilterData> ();

		int quota;

		[MonoTODO]
		public XPathMessageFilterTable ()
			: this (int.MaxValue)
		{
		}

		public XPathMessageFilterTable (int quota)
		{
			this.quota = quota;
		}

		[DataMember]
		public int NodeQuota {
			get { return quota; }
			set { quota = value; }
		}

		public TFilterData this [MessageFilter key] {
			get { return dict [key]; }
			set { dict [key] = value; }
		}

		public int Count {
			get { return dict.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public ICollection<MessageFilter> Keys {
			get { return dict.Keys; }
		}

		public ICollection<TFilterData> Values {
			get { return dict.Values; }
		}

		public void Add (KeyValuePair<MessageFilter,TFilterData> item)
		{
			dict.Add (item.Key, item.Value);
		}

		[MonoTODO]
		public void Add (XPathMessageFilter filter, TFilterData data)
		{
			throw new NotImplementedException ();
		}

		public void Add (MessageFilter filter, TFilterData data)
		{
			dict.Add (filter, data);
		}

		public void Clear ()
		{
			dict.Clear ();
		}

		public bool Contains (KeyValuePair<MessageFilter,TFilterData> item)
		{
			return dict.ContainsKey (item.Key) &&
				dict [item.Key].Equals (item.Value);
		}

		public bool ContainsKey (MessageFilter key)
		{
			return dict.ContainsKey (key);
		}

		public void CopyTo (KeyValuePair<MessageFilter,TFilterData> [] array, int index)
		{
			if (index < 0 || dict.Count >= array.Length - index)
				throw new ArgumentOutOfRangeException ("index");
			foreach (KeyValuePair<MessageFilter,TFilterData> item in dict)
				array [index++] = item;
		}

		public IEnumerator<KeyValuePair<MessageFilter,TFilterData>> GetEnumerator ()
		{
			return dict.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public bool GetMatchingFilter (Message message, out MessageFilter result)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilter (MessageBuffer buffer, out MessageFilter result)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilters (Message message, ICollection<MessageFilter> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilters (MessageBuffer buffer, ICollection<MessageFilter> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValue (Message message, out TFilterData data)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValue (MessageBuffer buffer, out TFilterData data)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValues (Message message, ICollection<TFilterData> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValues (MessageBuffer buffer, ICollection<TFilterData> results)
		{
			throw new NotImplementedException ();
		}

		public bool Remove (KeyValuePair<MessageFilter,TFilterData> item)
		{
			if (dict.ContainsKey (item.Key) && dict [item.Key].Equals (item.Value)) {
				dict.Remove (item.Key);
				return true;
			}
			return false;
		}

		public bool Remove (XPathMessageFilter filter)
		{
			throw new NotImplementedException ();
		}

		public bool Remove (MessageFilter filter)
		{
			return dict.Remove (filter);
		}

		public bool TryGetValue (MessageFilter filter, out TFilterData filterData)
		{
			if (dict.ContainsKey (filter)) {
				filterData = dict [filter];
				return true;
			} else {
				filterData = default (TFilterData);
				return false;
			}
		}

		[MonoTODO]
		public void TrimToSize ()
		{
			throw new NotImplementedException ();
		}
	}
}
