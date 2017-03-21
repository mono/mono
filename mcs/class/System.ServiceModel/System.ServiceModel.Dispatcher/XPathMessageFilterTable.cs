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

		public XPathMessageFilterTable (int capacity)
		{
			this.quota = capacity;
		}

		[DataMember]
		public int NodeQuota {
			get { return quota; }
			set { quota = value; }
		}

		public TFilterData this [MessageFilter filter] {
			get { return dict [filter]; }
			set { dict [filter] = value; }
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

		public bool ContainsKey (MessageFilter filter)
		{
			return dict.ContainsKey (filter);
		}

		public void CopyTo (KeyValuePair<MessageFilter,TFilterData> [] array, int arrayIndex)
		{
			if (arrayIndex < 0 || dict.Count >= array.Length - arrayIndex)
				throw new ArgumentOutOfRangeException ("arrayIndex");
			foreach (KeyValuePair<MessageFilter,TFilterData> item in dict)
				array [arrayIndex++] = item;
		}

		public IEnumerator<KeyValuePair<MessageFilter,TFilterData>> GetEnumerator ()
		{
			return dict.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public bool GetMatchingFilter (Message message, out MessageFilter filter)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilter (MessageBuffer messageBuffer, out MessageFilter filter)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilter (SeekableXPathNavigator navigator, out MessageFilter filter)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilter (XPathNavigator navigator, out MessageFilter filter)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilters (Message message, ICollection<MessageFilter> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilters (MessageBuffer messageBuffer, ICollection<MessageFilter> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilters (SeekableXPathNavigator navigator, ICollection<MessageFilter> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingFilters (XPathNavigator navigator, ICollection<MessageFilter> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValue (Message message, out TFilterData data)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValue (MessageBuffer messageBuffer, out TFilterData data)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValue (SeekableXPathNavigator navigator, out TFilterData data)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValue (XPathNavigator navigator, out TFilterData data)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValues (Message message, ICollection<TFilterData> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValues (MessageBuffer messageBuffer, ICollection<TFilterData> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValues (SeekableXPathNavigator navigator, ICollection<TFilterData> results)
		{
			throw new NotImplementedException ();
		}

		public bool GetMatchingValues (XPathNavigator navigator, ICollection<TFilterData> results)
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
			return dict.Remove (filter);
		}

		public bool Remove (MessageFilter filter)
		{
			return Remove ((XPathMessageFilter) filter);
		}

		static Exception trim_to_size_error;

		public void TrimToSize ()
		{
			// This is the documented behavior: throws NIE.
			if (trim_to_size_error == null)
				trim_to_size_error = new NotImplementedException ();
			throw trim_to_size_error;
		}

		public bool TryGetValue (MessageFilter filter, out TFilterData data)
		{
			return dict.TryGetValue (filter, out data);
		}
	}
}
