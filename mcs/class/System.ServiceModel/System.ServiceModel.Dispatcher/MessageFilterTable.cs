//
// System.ServiceModel.MessageFilterTable.cs
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

namespace System.ServiceModel.Dispatcher
{
	[DataContract]
	public class MessageFilterTable<TFilterData> : IMessageFilterTable<TFilterData>, IDictionary<MessageFilter,TFilterData>,
		ICollection<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable<KeyValuePair<MessageFilter,TFilterData>>, IEnumerable
	{

		int default_priority;
		Dictionary<MessageFilter, TFilterData> table;
		Dictionary<MessageFilter, int> priority_list;
		
		public MessageFilterTable ()
			: this (0) {} // 0 is the default

		public MessageFilterTable (int defaultPriority)
		{
			this.default_priority = defaultPriority;
			table = new Dictionary<MessageFilter, TFilterData> ();
			priority_list = new Dictionary<MessageFilter, int> ();
		}

		public void Add (KeyValuePair<MessageFilter, TFilterData> item)
		{
			this.Add (item.Key, item.Value, default_priority);
		}

		public void Add (MessageFilter filter, TFilterData data)
		{
			this.Add (filter, data, default_priority);
		}

		public void Add (MessageFilter filter, TFilterData data, int priority)
		{
			table.Add (filter, data);
			priority_list.Add (filter, priority);
		}

		public void Clear ()
		{
			table.Clear ();
			priority_list.Clear ();
		}

		public bool Contains (KeyValuePair<MessageFilter, TFilterData> item)
		{
			return ContainsKey (item.Key);
		}

		public bool ContainsKey (MessageFilter filter)
		{
			return table.ContainsKey (filter);
		}

		public void CopyTo (KeyValuePair<MessageFilter, TFilterData> [] array, int arrayIndex)
		{
			((ICollection) table).CopyTo (array, arrayIndex);
		}

		protected virtual IMessageFilterTable<TFilterData> CreateFilterTable (MessageFilter filter)
		{
			return filter.CreateFilterTable<TFilterData> ();
		}

		public IEnumerator<KeyValuePair<MessageFilter, TFilterData>> GetEnumerator ()
		{
			return table.GetEnumerator ();
		}

		public int GetPriority (MessageFilter filter)
		{
			return priority_list [filter];
		}

		public bool GetMatchingFilter (Message message, out MessageFilter filter)
		{
			if (message == null)
				throw new ArgumentNullException ("message is null");
			filter = null;

			int count = 0;
			foreach (MessageFilter f in table.Keys) {
				if (!f.Match (message))
					continue;

				if (count > 1)
					throw new MultipleFilterMatchesException (
					    "More than one filter matches.");

				count++;
				filter = f;
			}
			return count == 1;
		}

		public bool GetMatchingFilter (MessageBuffer buffer, out MessageFilter filter)
		{
			return GetMatchingFilter (buffer.CreateMessage (), out filter);
		}

		[MonoTODO ("Consider priority")]		
		public bool GetMatchingFilters (Message message, ICollection<MessageFilter> results)
		{
			if (message == null)
				throw new ArgumentNullException ("message is null.");

			int count = 0;
			foreach (MessageFilter f in table.Keys)
				if (f.Match (message)) {
					results.Add (f);
					count++;
				}
			return count != 0;
		}

		public bool GetMatchingFilters (MessageBuffer buffer, ICollection<MessageFilter> results)
		{
			return GetMatchingFilters (buffer.CreateMessage (), results);
		}

		[MonoTODO ("Consider priority")]
		public bool GetMatchingValue (Message message, out TFilterData data)
		{
			if (message == null)
				throw new ArgumentNullException ("message is null");
			data = default (TFilterData);

			int count = 0;
			foreach (MessageFilter f in table.Keys) {
				if (!f.Match (message))
					continue;

				if (count > 1)
					throw new MultipleFilterMatchesException (
					    "More than one filter matches.");

				count++;
				data = table [f];
			}
			return count == 1;
		}

		public bool GetMatchingValue (MessageBuffer buffer, out TFilterData data)
		{
			return GetMatchingValue (buffer.CreateMessage (), out data);
		}

		public bool GetMatchingValues (Message message, ICollection<TFilterData> results)
		{
			if (message == null)
				throw new ArgumentNullException ("message is null.");

			int count = 0;
			foreach (MessageFilter f in table.Keys)
				if (f.Match (message)) {
					results.Add (table [f]);
					count++;
				}
			return count != 0;
		}

		public bool GetMatchingValues (MessageBuffer buffer, ICollection<TFilterData> results)
		{
			return GetMatchingValues (buffer.CreateMessage (), results);
		}

		public bool Remove (MessageFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException ("filter is null.");

			if (!table.ContainsKey (filter))
				return false;
			
			table.Remove (filter);
			priority_list.Remove (filter);
			return true;
		}

		public bool Remove (KeyValuePair<MessageFilter, TFilterData> item)
		{
			return Remove (item.Key);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator) table.GetEnumerator ();
		}

		public bool TryGetValue (MessageFilter filter, out TFilterData data)
		{
			return table.TryGetValue (filter, out data);
		}

		public int Count { get { return table.Count; } }

		[DataMember]
		public int DefaultPriority {
			get { return default_priority; }
			set { default_priority = value; }
		}

		public bool IsReadOnly { get { return false; } }

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