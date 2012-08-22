using System;

namespace Mono.CodeContracts.Static.DataStructures
{
	static class ArrayExtensions
	{
		public static bool Contains<T> (this T[] array, T element)
		{
			foreach(T item in array)
			{
				if(item.Equals((object) element)) return true;
			}
			return false;
		}
	}
}

