// 
// APCMap.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.StackAnalysis {
	class APCMap<T> {
		private readonly Dictionary<int, T>[] block_map;
		private IImmutableIntMap<bool> call_on_this_map;

		public APCMap (Subroutine parent)
		{
			this.block_map = new Dictionary<int, T>[parent.BlockCount];
			this.call_on_this_map = ImmutableIntMap<bool>.Empty ();
		}

		public T this [APC key]
		{
			get
			{
				T value;
				if (!TryGetValue (key, out value))
					throw new KeyNotFoundException ();
				return value;
			}
		}

		public void Add (APC pc, T value)
		{
			Dictionary<int, T> pcBlockMap = this.block_map [pc.Block.Index];
			if (pcBlockMap == null)
				this.block_map [pc.Block.Index] = pcBlockMap = new Dictionary<int, T> ();

			pcBlockMap.Add (pc.Index, value);
		}

		public bool TryGetValue (APC pc, out T obj)
		{
			Dictionary<int, T> dictionary = this.block_map [pc.Block.Index];
			if (dictionary != null)
				return dictionary.TryGetValue (pc.Index, out obj);

			obj = default(T);
			return false;
		}

		public bool ContainsKey (APC pc)
		{
			Dictionary<int, T> dictionary = this.block_map [pc.Block.Index];
			if (dictionary == null)
				return false;
			return dictionary.ContainsKey (pc.Index);
		}

		public bool IsCallOnThis (APC key)
		{
			return this.call_on_this_map [key.Block.Index];
		}

		public void AddCallOnThis (APC pc)
		{
			this.call_on_this_map = this.call_on_this_map.Add (pc.Block.Index, true);
		}
	}
}
