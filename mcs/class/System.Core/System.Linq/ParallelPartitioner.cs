//
// ParallelPartitioner.cs
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
using System.Collections.Generic;
using System.Collections.Concurrent;

#if NET_4_0

namespace System.Linq
{
	internal static class ParallelPartitioner
	{
		internal static OrderablePartitioner<T> CreateForChunks<T> (IEnumerable<T> source)
		{
			return Partitioner.Create (source);
		}

		internal static OrderablePartitioner<T> CreateForRange<T> (IList<T> source)
		{
			return Partitioner.Create (source, true);
		}

		internal static OrderablePartitioner<T> CreateBest<T> (IEnumerable<T> source)
		{
			IList<T> temp = source as IList<T>;
			if (temp != null)
				return CreateForRange (temp);

			return CreateForChunks (source);
		}

		internal static OrderablePartitioner<T> CreateForStrips<T> (IEnumerable<T> source, int stripSize)
		{
			IList<T> temp = source as IList<T>;
			if (temp != null)
				return new StripPartitioner<T> (temp);

			return new EnumerablePartitioner<T> (source, stripSize, 1);
		}

		internal static OrderablePartitioner<int> CreateForRange (int start, int count)
		{
			return CreateForRange<int> (new RangeList (start, count));
		}

		internal static OrderablePartitioner<T> CreateForRepeat<T> (T obj, int count)
		{
			return CreateForRange<T> (new RepeatList<T> (obj, count));
		}
	}
}
#endif
