//
// System.Web.Caching.CacheItem
//
// Author(s):
//  Lluis Sanchez <lluis@ximian.com>
//
// (C) 2005-2010 Novell, Inc (http://novell.com)
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

namespace System.Web.Caching
{
	sealed class CacheItemEnumerator: IDictionaryEnumerator
	{
		List <CacheItem> list;
		int pos = -1;
		
		public CacheItemEnumerator (List <CacheItem> list)
		{
			this.list = list;
		}
		
		CacheItem Item {
			get {
				if (pos < 0 || pos >= list.Count)
					throw new InvalidOperationException ();
				return list [pos];
			}
		}
		
		public DictionaryEntry Entry {
			get {
				CacheItem item = Item;
				if (item == null)
					return new DictionaryEntry (null, null);
				
				return new DictionaryEntry (item.Key, item.Value);
			}
		}
		
		public object Key {
			get { return Item.Key; }
		}
		
		public object Value {
			get { return Item.Value; }
		}
		
		public object Current {
			get { return Entry; }
		}
		
		public bool MoveNext ()
		{
			return (++pos < list.Count);
		}
		
		public void Reset ()
		{
			pos = -1;
		}
	}
}
