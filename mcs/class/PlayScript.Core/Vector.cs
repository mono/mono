//
// Vector.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2013 Xamarin, Inc (http://www.xamarin.com)
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
using PlayScript.Runtime.CompilerServices;

namespace _root
{
	[DynamicClass]
	public class Vector<T>
	{
		readonly List<T> list;
		
		public Vector (int length = 0, bool @fixed = false)
		{
			list = new List<T> (length);
			this.@fixed = @fixed;
		}

		public Vector (params T[] values)
		{
			list = new List<T> (values);
		}

		private Vector (List<T> list)
		{
			this.list = list;
		}
		
		public bool @fixed { get; set; } 
		
		public uint length {
			get {
				return (uint) list.Count;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public Vector<T> concat ([RestArrayParameter] Array args)
		{
			throw new NotImplementedException ();
		}

		public Vector<T> concat (params T[] args)
		{
			var res = new List<T> (list);
			res.AddRange (args);
			return new Vector<T> (res);
		}

		public int indexOf (T searchElement, int fromIndex)
		{
			return list.IndexOf (searchElement, fromIndex);
		}

		public int lastIndexOf (T searchElement, int fromIndex = 0x7fffffff)
		{
			return list.LastIndexOf (searchElement, fromIndex);			
		}
		
		public uint push (T value)
		{
			list.Add (value);
			return length;
		}

		public uint unshift ([RestArrayParameter] Array args)
		{
			throw new NotImplementedException ();
		}

		public uint unshift (params T[] args)
		{
			list.InsertRange (0, args);
			return length;
		}

		internal void setValue (uint index, T value)
		{
			if (index < length) {
				list [(int) index] = value;
				return;
			}

//			while (length < index - 1)
//				list.Add (default (T)); // TODO: Need better "invisible" value, perhaps DELETED?

			list.Add (value);
		}		
	}
}

