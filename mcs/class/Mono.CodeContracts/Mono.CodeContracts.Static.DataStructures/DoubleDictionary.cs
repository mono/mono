// 
// DoubleDictionary.cs
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

using System.Collections.Generic;
using System.Linq;

namespace Mono.CodeContracts.Static.DataStructures {
	class DoubleDictionary<A, B, C> : Dictionary<A, Dictionary<B, C>> {
		public C this [A a, B b]
		{
			get { return base [a] [b]; }
			set
			{
				Dictionary<B, C> dict;
				if (!base.TryGetValue (a, out dict)) {
					dict = new Dictionary<B, C> ();
					base.Add (a, dict);
				}
				dict [b] = value;
			}
		}

		public IEnumerable<A> Keys1
		{
			get { return base.Keys; }
		}

		public void Add (A a, B b, C value)
		{
			Dictionary<B, C> dict;
			if (!base.TryGetValue (a, out dict)) {
				dict = new Dictionary<B, C> ();
				base.Add (a, dict);
			}
			dict.Add (b, value);
		}

		public IEnumerable<B> Keys2 (A a)
		{
			Dictionary<B, C> dict;
			if (base.TryGetValue (a, out dict))
				return dict.Keys;
			return Enumerable.Empty<B> ();
		}

		public bool ContainsKey (A a, B b)
		{
			Dictionary<B, C> dict;
			if (!base.TryGetValue (a, out dict))
				return false;

			return dict.ContainsKey (b);
		}

		public bool TryGetValue (A a, B b, out C value)
		{
			Dictionary<B, C> dict;
			if (base.TryGetValue (a, out dict))
				return dict.TryGetValue (b, out value);

			value = default(C);
			return false;
		}
	}
}
