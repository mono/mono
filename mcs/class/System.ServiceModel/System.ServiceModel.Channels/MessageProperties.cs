//
// MessageProperties.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

using Pair = System.Collections.Generic.KeyValuePair<string, object>;

namespace System.ServiceModel.Channels
{
	public sealed class MessageProperties : IDictionary<string, object>, 
		ICollection<Pair>, IEnumerable<Pair>, IEnumerable, IDisposable
	{
		List<Pair> list;

		public MessageProperties ()
		{
			list = new List<Pair> ();
		}

		public MessageProperties (MessageProperties properties)
		{
			list = new List<Pair> ();
			CopyProperties (properties);
		}

		public bool AllowOutputBatching {
			get {
				var obj = this ["AllowOutputBatching"];
				return obj != null ? (bool) obj : false;
			}
			set { this ["AllowOutputBatching"] = value; }
		}

		public int Count {
			get { return list.Count; }
		}

		public MessageEncoder Encoder {
			get { return (MessageEncoder) this ["Encoder"]; }
			set { this ["Encoder"] = value; }
		}

		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public ICollection<string> Keys {
			get { return new ParameterKeyCollection (list); }
		}

		public object this [string name] {
			get {
				for (int i = 0; i < list.Count; i++)
					if (list [i].Key == name)
						return list [i].Value;
				return null;
			}
			set {
				for (int i = 0; i < list.Count; i++)
					if (list [i].Key == name) {
						list [i] = new Pair (name, value);
						return;
					}
				list.Add (new Pair (name, value));
			}
		}

#if !MOBILE
		public SecurityMessageProperty Security {
			get { return (SecurityMessageProperty) this ["Security"]; }
			set { this ["Security"] = value; }
		}
#endif

		public ICollection<object> Values {
			get { return new ParameterValueCollection (list); }
		}

		public Uri Via {
			get { return (Uri) this ["Via"]; }
			set { this ["Via"] = value; }
		}

		public void Add (string name, object property)
		{
			list.Add (new Pair (name, property));
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool ContainsKey (string name)
		{
			for (int i = 0; i < list.Count; i++)
				if (list [i].Key == name)
					return true;
			return false;
		}

		public void CopyProperties (MessageProperties properties)
		{
			list.AddRange (properties.list);
		}

		public void Dispose ()
		{
		}

		public bool Remove (string name)
		{
			for (int i = 0; i < list.Count; i++)
				if (list [i].Key == name) {
					list.RemoveAt (i);
					return true;
				}
			return false;
		}

		public bool TryGetValue (string name, out object value)
		{
			for (int i = 0; i < list.Count; i++)
				if (list [i].Key == name) {
					value = list [i].Value;
					return true;
				}
			value = null;
			return false;
		}

		void ICollection<Pair>.Add (Pair pair)
		{
			list.Add (pair);
		}

		bool ICollection<Pair>.Contains (Pair pair)
		{
			return list.Contains (pair);
		}

		void ICollection<Pair>.CopyTo (Pair [] array, int index)
		{
			list.CopyTo (array, index);
		}

		bool ICollection<Pair>.Remove (Pair pair)
		{
			return list.Remove (pair);
		}

		IEnumerator<Pair> IEnumerable<Pair>.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator) ((IEnumerable<Pair>) this).GetEnumerator ();
		}

		class ParameterKeyCollection : ICollection<string>
		{
			List<Pair> source;

			public ParameterKeyCollection (List<Pair> source)
			{
				this.source = source;
			}

			public int Count {
				get { return source.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public void Add (string item)
			{
				throw new InvalidOperationException ();
			}

			public void Clear ()
			{
				throw new InvalidOperationException ();
			}

			public bool Contains (string item)
			{
				for (int i = 0; i < source.Count; i++)
					if (source [i].Key == item)
						return true;
				return false;
			}

			public void CopyTo (string [] array, int index)
			{
				for (int i = 0; i < source.Count; i++)
					array [index + i] = source [i].Key;
			}

			public IEnumerator<string> GetEnumerator ()
			{
				foreach (Pair p in source)
					yield return p.Key;
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				foreach (Pair p in source)
					yield return p.Key;
			}

			public bool Remove (string item)
			{
				throw new InvalidOperationException ();
			}
		}

		class ParameterValueCollection : ICollection<object>
		{
			List<Pair> source;

			public ParameterValueCollection (List<Pair> source)
			{
				this.source = source;
			}

			public int Count {
				get { return source.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public void Add (object item)
			{
				throw new InvalidOperationException ();
			}

			public void Clear ()
			{
				throw new InvalidOperationException ();
			}

			public bool Contains (object item)
			{
				for (int i = 0; i < source.Count; i++)
					if (source [i].Value == item)
						return true;
				return false;
			}

			public void CopyTo (object [] array, int index)
			{
				for (int i = 0; i < source.Count; i++)
					array [index + i] = source [i].Value;
			}

			public IEnumerator<object> GetEnumerator ()
			{
				foreach (Pair p in source)
					yield return p.Value;
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				foreach (Pair p in source)
					yield return p.Key;
			}

			public bool Remove (object item)
			{
				throw new InvalidOperationException ();
			}
		}
	}
}
