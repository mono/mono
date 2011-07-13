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
