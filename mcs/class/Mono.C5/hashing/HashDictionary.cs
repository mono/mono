/*
 Copyright (c) 2003-2004 Niels Kokholm <kokholm@itu.dk> and Peter Sestoft <sestoft@dina.kvl.dk>
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using System.Diagnostics;
using MSG = System.Collections.Generic;

namespace C5
{
	/// <summary>
	/// A generic dictionary class based on a hash set class <see cref="T:C5.HashSet!1"/>.
	/// </summary>
	public class HashDictionary<K,V>: DictionaryBase<K,V>, IDictionary<K,V>
	{
		/// <summary>
		/// Create a hash dictionary using a default hasher for the keys.
		/// Initial capacity of internal table will be 16 entries and threshold for 
		/// expansion is 66% fill.
		/// </summary>
		public HashDictionary()
		{
			pairs = new HashSet<KeyValuePair<K,V>>(new KeyValuePairHasher<K,V>());
		}

		/// <summary>
		/// Create a hash dictionary using a custom hasher for the keys.
		/// Initial capacity of internal table will be 16 entries and threshold for 
		/// expansion is 66% fill.
		/// </summary>
		/// <param name="h">The external key hasher</param>
		public HashDictionary(IHasher<K> h)
		{
			pairs = new HashSet<KeyValuePair<K,V>>(new KeyValuePairHasher<K,V>(h));
		}


		/// <summary>
		/// Create a hash dictionary using a custom hasher and prescribing the 
		/// initial size of the dictionary and a non-default threshold for internal table expansion.
		/// </summary>
		/// <param name="capacity">The initial capacity. Will be rounded upwards to nearest
		/// power of 2, at least 16.</param>
		/// <param name="fill">The expansion threshold. Must be between 10% and 90%.</param>
		/// <param name="h">The external key hasher</param>
		public HashDictionary(int capacity, double fill, IHasher<K> h)
		{
			KeyValuePairHasher<K,V> kvph = new KeyValuePairHasher<K,V>(h);

			pairs = new HashSet<KeyValuePair<K,V>>(capacity, fill, kvph);
		}
	}
}