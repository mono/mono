#if NET_4_0
//
// QueryDefaultEmptyNode.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	internal class QueryDefaultEmptyNode<TSource> : QueryStreamNode<TSource, TSource>
	{
		TSource defaultValue;
		
		internal QueryDefaultEmptyNode (QueryBaseNode<TSource> parent, TSource defaultValue)
			: base (parent, false)
		{
			this.defaultValue = defaultValue;
		}
		
		internal override IEnumerable<TSource> GetSequential ()
		{
			return Parent.GetSequential ().DefaultIfEmpty (defaultValue);
		}
		
		internal override IList<IEnumerable<TSource>> GetEnumerables (QueryOptions options)
		{
			IList<IEnumerable<TSource>> enumerables = Parent.GetEnumerables (options);
			CountdownEvent evt = new CountdownEvent (enumerables.Count);

			return enumerables
				.Select ((e) => GetEnumerableInternal<TSource> (e,
				                                                evt,
				                                                (s) => s))
				.ToArray ();
		}
		
		internal override IList<IEnumerable<KeyValuePair<long, TSource>>> GetOrderedEnumerables (QueryOptions options)
		{
			IList<IEnumerable<KeyValuePair<long, TSource>>> enumerables = Parent.GetOrderedEnumerables (options);
			CountdownEvent evt = new CountdownEvent (enumerables.Count);

			return enumerables
				.Select ((e) => GetEnumerableInternal<KeyValuePair<long, TSource>> (e,
				                                                                    evt,
				                                                                    (s) => new KeyValuePair<long, TSource> (0, s)))
				.ToArray ();
		}
		
		IEnumerable<TSecond> GetEnumerableInternal<TSecond> (IEnumerable<TSecond> source, 
		                                                     CountdownEvent evt,
		                                                     Func<TSource, TSecond> converter)
		{
			bool processed = false;
			
			foreach (TSecond second in source) {
				processed = true;
				yield return second;
			}
			
			if (!processed && evt.Signal ())
				yield return converter (defaultValue);
		}
	}
}

#endif
