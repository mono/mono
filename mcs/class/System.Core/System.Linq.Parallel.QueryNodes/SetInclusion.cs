//
// SetInclusion.cs
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

#if NET_4_0
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq.Parallel.QueryNodes
{
	[Flags]
	internal enum SetInclusion {
		In = 0, // Element should already be in checker
		Out = 1, // Element shouldn't be already in checker
		Preload = 2, // Preload second node content and only do real work on first node content
		Relaxed = 4, // first node content isn't added to checker allowing duplicates
	}

	internal static class SetInclusionDefaults
	{
		public const SetInclusion Union = SetInclusion.Out;
		public const SetInclusion Intersect = SetInclusion.In;
		public const SetInclusion Except = SetInclusion.In | SetInclusion.Preload | SetInclusion.Relaxed;
		public const SetInclusion Distinct = SetInclusion.Out | SetInclusion.Preload;
	}
}

#endif