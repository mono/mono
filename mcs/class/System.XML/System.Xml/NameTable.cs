//
// System.Xml.NameTable.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) Ximian, Inc.
// (C) 2003 Ben Maurer
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

namespace System.Xml {

	//
	// This class implements the name table as a simple
	// hashtable, using buckets and a linked list.
	//
	public class NameTable : XmlNameTable {
		
		const int INITIAL_BUCKETS = 2 << 6; // 64
		
		int count = INITIAL_BUCKETS;
		Entry [] buckets = new Entry [INITIAL_BUCKETS];
		int size;

		public NameTable () {}
		
		class Entry {
			public string str;
			public int hash, len;
			public Entry next;
	
			public Entry (string str, int hash, Entry next)
			{
				this.str = str;
				this.len = str.Length;
				this.hash = hash;
				this.next = next;
			}
		}
		
		public override string Add (char [] key, int start, int len)
		{
			if (((0 > start) && (start >= key.Length))
			   || ((0 > len) && (len >= key.Length - len)))
				throw new IndexOutOfRangeException ("The Index is out of range.");
			
			if (len == 0) return String.Empty;
			
			int h = 0;
			// This is from the String.Gethash () icall
			int end = start + len;
			for (int i = start; i < end; i++)
				h = (h << 5) - h + key [i];
			// h must be be >= 0
			h &= 0x7FFFFFFF;
			

			for (Entry e = buckets [h % count]; e != null; e = e.next) {
				if (e.hash == h && e.len == len && StrEqArray (e.str, key, start))
					return e.str;
			}
			return AddEntry (new string (key, start, len), h);
		}
		
		public override string Add (string key)
		{
			if (key == null) throw new ArgumentNullException ("key");

			int keyLen = key.Length;
			if (keyLen == 0) return String.Empty;

			int h = 0;
			// This is from the String.Gethash () icall
			for (int i = 0; i < keyLen; i++)
				h = (h << 5) - h + key [i];
			
			// h must be be >= 0
			h &= 0x7FFFFFFF;

			for (Entry e = buckets [h % count]; e != null; e = e.next) {
				if (e.hash == h && e.len == key.Length && e.str == key)
					return e.str;
			}
			
			return AddEntry (key, h);
		}

		public override string Get (char [] key, int start, int len)
		{
			if (((0 > start) && (start >= key.Length))
			   || ((0 > len) && (len >= key.Length - len)))
				throw new IndexOutOfRangeException ("The Index is out of range.");
			
			if (len == 0) return String.Empty;
			
			int h = 0;
			// This is from the String.Gethash () icall
			int end = start + len;
			for (int i = start; i < end; i++)
				h = (h << 5) - h + key [i];
			// h must be be >= 0
			h &= 0x7FFFFFFF;
			
			for (Entry e = buckets [h % count]; e != null; e = e.next) {
				if (e.hash == h && e.len == len && StrEqArray (e.str, key, start))
					return e.str;
			}
			
			return null;
		}
		
		public override string Get (string value) {
			if (value == null) throw new ArgumentNullException ("value");

			int valLen = value.Length;
			if (valLen == 0) return String.Empty;
				
			int h = 0;
			// This is from the String.Gethash () icall
			for (int i = 0; i < valLen; i++)
				h = (h << 5) - h + value [i];
			// h must be be >= 0
			h &= 0x7FFFFFFF;
			
			for (Entry e = buckets [h % count]; e != null; e = e.next) {
				if (e.hash == h && e.len == value.Length && e.str == value)
					return e.str;
			}
			
			return null;
		}
		
		string AddEntry (string str, int hash)
		{
			int bucket = hash % count;
			buckets [bucket] = new Entry (str, hash, buckets [bucket]);
			
			// Grow whenever we double in size
			if (size++ == count) {
				count <<= 1;
				int csub1 = count - 1;
				
				Entry [] newBuckets = new Entry [count];
				for (int i = 0; i < buckets.Length; i++) {
					Entry root = buckets [i];
					Entry e = root;
					while (e != null) {
						int newLoc = e.hash & csub1;
						Entry n = e.next;
						e.next = newBuckets [newLoc];
						newBuckets [newLoc] = e;
						e = n;
					}
				}

				buckets = newBuckets;
			}
			
			return str;
		}
	
		static bool StrEqArray (string str, char [] str2, int start)
		{
			int i = str.Length;
			i --;
			start += i;
			do
			{
				if (str[i] != str2[start])
					return false;

				i--;
				start--;
			}
			while(i >= 0);

			return true;
		}
	}
}
