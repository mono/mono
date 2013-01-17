// 
// DecoratorHelper.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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
using System.Collections.Generic;

namespace Mono.CodeContracts.Static.DataStructures {
	/// <summary>
	/// This class is used with subroutines to substitute IStackInfo and IEdgeSubroutineAdapter to desired
	/// </summary>
	static class DecoratorHelper {
		private static readonly List<object> ContextAdapters = new List<object> ();

		private static object Last
		{
			get { return ContextAdapters [ContextAdapters.Count - 1]; }
		}

		public static void Push<T> (T @this) where T : class
		{
			ContextAdapters.Add (@this);
		}

		public static void Pop ()
		{
			ContextAdapters.RemoveAt (ContextAdapters.Count - 1);
		}

		public static T Dispatch<T> (T @this) where T : class
		{
			return FindAdaptorStartingAt (@this, 0);
		}

		private static T FindAdaptorStartingAt<T> (T @default, int startIndex)
			where T : class
		{
			List<object> list = ContextAdapters;
			for (int i = startIndex; i < list.Count; ++i) {
				var obj = list [i] as T;
				if (obj != null)
					return obj;
			}
			return @default;
		}

		public static T Inner<T> (T @this) where T : class
		{
			for (int i = 0; i < ContextAdapters.Count; i++) {
				if (ContextAdapters [i] == @this) {
					ClearDuplicates (@this, i + 1);
					T inner = FindAdaptorStartingAt (default(T), i + 1);
					if (inner != null)
						return inner;

					throw new InvalidOperationException ("No inner context found");
				}
			}

			throw new InvalidOperationException ("@this is not current adaptor");
		}

		private static void ClearDuplicates (object @this, int @from)
		{
			for (int i = from; i < ContextAdapters.Count; i++) {
				if (ContextAdapters [i] == @this)
					ContextAdapters [i] = null;
			}
		}
	}
}
