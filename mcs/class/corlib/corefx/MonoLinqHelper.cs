// Helper class providing missing System.Linq functionality
// for CoreFX code

using System;
using System.Collections.Generic;

namespace System.IO
{
	internal static class MonoLinqHelper
	{
		public static T[] ToArray<T>(this IEnumerable<T> source)
		{
			return EnumerableHelpers.ToArray<T> (source);
		}
	}
}
