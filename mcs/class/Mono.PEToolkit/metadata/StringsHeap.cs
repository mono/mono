/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.Text;
using System.Collections;

namespace Mono.PEToolkit.Metadata {

	// 23.1.3
	public class StringsHeap : MDHeap {

		private SortedList strings;
		private int dataLen;

		internal StringsHeap(MDStream stream) : base(stream)
		{
		}

		unsafe override public void FromRawData(byte[] rawData)
		{
			strings = new SortedList();
			strings.Add (0, String.Empty);
			if (rawData == null) return;

			int len = rawData.Length;
			dataLen = len;
			// the first entry in the string heap is always EmptyString
			if (len < 1 || rawData [0] != 0) {
				throw new BadMetaDataException("Invalid #Strings heap.");
			}

			int idx = 1;
			for (int i = 1; i < len; i++) {
				if (rawData [i] == 0) {
					fixed (void* p = &rawData[idx]) {
						string s = PEUtils.GetString ((sbyte*)p, 0, i - idx, Encoding.UTF8);
						strings.Add (idx, s);
					}
					idx = i + 1;
				}
			}
		}

		public string this [int index] {
			get {
				string res = null;
				if (strings != null && index >= 0 && index < dataLen) {
					res = strings[index] as string;
					if (res == null) {
						// cope with garbage/substrings
						IList indices = strings.GetKeyList();
						int i = FindNextIndex(indices, index);
						if (i < 0) {
							throw new Exception("Internal error (#Strings binary search).");
						}
						if (i != 0) {
							// Position of the super-string in the heap.
							int pos = (int) indices [i - 1];
							res = strings[pos] as string;
							// NOTE: Substring returns String.Empty if index
							// is equal to the length.
							res = res.Substring(index - pos);
						}
					}
				}
				return res;
			}
		}

		/// <summary>
		/// Binary search.
		/// </summary>
		/// <param name="list">List of "edge" indices.</param>
		/// <param name="index"></param>
		/// <returns></returns>
		private static int FindNextIndex(IList list, int index) {
			int len = list.Count;

			if (len == 0) return ~0;

			int left = 0;
			int right = len-1;

			while (left <= right) {
				int guess = (left + right) >> 1;
				int cmp = index - (int) list [guess];
				if (cmp == 0) return ~guess;
				cmp &= ~Int32.MaxValue;
				if (cmp == 0) left = guess+1;
				else right = guess-1;
			}

			return left;
		}

		public int Count {
			get {
				return (strings == null) ? 0 : strings.Count;
			}
		}

	}
}
