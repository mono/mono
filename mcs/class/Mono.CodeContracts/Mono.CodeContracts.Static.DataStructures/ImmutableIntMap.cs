// 
// ImmutableIntMap.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

namespace Mono.CodeContracts.Static.DataStructures {
	class ImmutableIntMap<T> : IImmutableIntMap<T> {
		private readonly Dictionary<int, T> map = new Dictionary<int, T> ();

		private ImmutableIntMap (Dictionary<int, T> map)
		{
			this.map = map;
		}

		#region Implementation of IImm	utableIntMap<T>
		public T this [int key]
		{
			get
			{
				if (this.map == null || !this.map.ContainsKey (key))
					return default(T);

				return this.map [key];
			}
		}

		public T Any
		{
			get
			{
				if (this.map == null)
					return default(T);
				return this.map.Values.First ();
			}
		}

		public IEnumerable<T> Values
		{
			get
			{
				if (this.map == null)
					return Enumerable.Empty<T> ();
				return this.map.Values;
			}
		}

		public IEnumerable<int> Keys
		{
			get
			{
				if (this.map == null)
					return Enumerable.Empty<int> ();
				return this.map.Keys;
			}
		}

		public int Count
		{
			get
			{
				if (this.map == null)
					return 0;
				return this.map.Count;
			}
		}

		public T Lookup (int key)
		{
			if (this.map == null)
				return default(T);
			return this.map [key];
		}

		public bool Contains (int key)
		{
			if (this.map == null)
				return false;
			return this.map.ContainsKey (key);
		}

		public IImmutableIntMap<T> Add (int key, T value)
		{
			if (this.map == null)
				return new ImmutableIntMap<T> (new Dictionary<int, T> {{key, value}});

			var newDict = new Dictionary<int, T> (this.map);
			newDict [key] = value;
			return new ImmutableIntMap<T> (newDict);
		}

		public IImmutableIntMap<T> Remove (int key)
		{
			if (this.map == null || !this.map.ContainsKey (key))
				return this;

			var newDict = new Dictionary<int, T> (this.map);
			newDict.Remove (key);
			return new ImmutableIntMap<T> (newDict);
		}

		public void Visit (Action<T> action)
		{
			foreach (T value in Values)
				action (value);
		}

		public void Visit (Action<int, T> action)
		{
			foreach (int key in Keys)
				action (key, this [key]);
		}
		#endregion

		public static IImmutableIntMap<T> Empty ()
		{
			return new ImmutableIntMap<T> (null);
		}
	}
}
