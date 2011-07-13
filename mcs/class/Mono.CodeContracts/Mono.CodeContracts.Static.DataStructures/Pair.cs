// 
// Pair.cs
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

namespace Mono.CodeContracts.Static.DataStructures {
	class Pair<K, V> : IEquatable<Pair<K, V>> {
		private static readonly bool KeyIsReferenceType = default(K) == null;
		private static readonly bool ValueIsReferenceType = default(V) == null;

		public readonly K Key;
		public readonly V Value;

		public Pair (K key, V value)
		{
			this.Key = key;
			this.Value = value;
		}

		#region IEquatable<Pair<K,V>> Members
		public bool Equals (Pair<K, V> other)
		{
			var keyEquatable = ((object) this.Key) as IEquatable<K>;
			bool keysAreEqual = keyEquatable != null ? keyEquatable.Equals (other.Key) : Equals (this.Key, other.Key);
			if (!keysAreEqual)
				return false;

			var valueEquatable = ((object) this.Value) as IEquatable<V>;
			return valueEquatable != null ? valueEquatable.Equals (other.Value) : Equals (this.Value, other.Value);
		}
		#endregion

		public override int GetHashCode ()
		{
			return (!KeyIsReferenceType || ((object) this.Key) != null ? this.Key.GetHashCode () : 0)
			       + 13*(!ValueIsReferenceType || ((object) this.Value) != null ? this.Value.GetHashCode () : 0);
		}

		public override string ToString ()
		{
			return string.Format ("({0}, {1})",
			                      (!KeyIsReferenceType || ((object) this.Key) != null) ? this.Key.ToString () : "<null>",
			                      (!ValueIsReferenceType || ((object) this.Value) != null) ? this.Value.ToString () : "<null>");
		}
	}
}
