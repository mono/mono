//
// System.ServiceModel.ActionMessageFilterTable.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Dispatcher {

	[DataContract]
	internal class ActionMessageFilterTable<TFilterData>
		: IMessageFilterTable<TFilterData>, 
		  IDictionary<MessageFilter,TFilterData>,
		  ICollection<KeyValuePair<MessageFilter, TFilterData>>, 
		  IEnumerable<KeyValuePair<MessageFilter,TFilterData>>, 
		  IEnumerable
	{
		Dictionary<MessageFilter, TFilterData> table;
		
		public ActionMessageFilterTable ()
		{
			table = new Dictionary<MessageFilter, TFilterData> ();
		}

		public void Add (KeyValuePair<MessageFilter, TFilterData> item)
		{
			table.Add (item.Key, item.Value);
		}

		public void Add (ActionMessageFilter filter, TFilterData data)
		{
			table.Add (filter, data);
		}

		public void Add (MessageFilter filter, TFilterData data)
		{
			table.Add (filter, data);
		}

		public void Clear ()
		{
			table.Clear ();
		}

		public bool Contains (KeyValuePair<MessageFilter, TFilterData> item)
		{
			return table.ContainsKey (item.Key);
		}

		public bool ContainsKey (MessageFilter filter)
		{
			return table.ContainsKey (filter);
		}

		public void CopyTo (KeyValuePair<MessageFilter, TFilterData> [] array, int index)
		{
			((ICollection<KeyValuePair<MessageFilter, TFilterData>>) table).CopyTo (array, index);
		}

		public IEnumerator<KeyValuePair<MessageFilter, TFilterData>> GetEnumerator ()
		{
			return ((ICollection<KeyValuePair<MessageFilter, TFilterData>>) table).GetEnumerator ();
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

		public bool Remove (ActionMessageFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException ("filter is null.");

			return table.Remove (filter);
		}

		public bool Remove (MessageFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException ("filter is null.");

			return table.Remove (filter);
		}

		public bool Remove (KeyValuePair<MessageFilter, TFilterData> item)
		{
			return table.Remove (item.Key);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return table.GetEnumerator ();
		}

		public bool TryGetValue (MessageFilter filter, out TFilterData data)
		{
			if (table.ContainsKey (filter)){
				data = table [filter];
				return true;
			}

			data = default (TFilterData);
			return false;
		}

		public int Count { get { return table.Count; }}

		public bool IsReadOnly { get { return false; }}

		public TFilterData this [MessageFilter filter] {
			get { return table [filter]; }
			set { table [filter] = value; }
		}

		public ICollection<MessageFilter> Keys {
			get { return table.Keys; }
		}

		public ICollection<TFilterData> Values {
			get { return table.Values; }
		}
	}
}