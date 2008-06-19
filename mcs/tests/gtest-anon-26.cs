using System;
using System.Collections.Generic;

namespace MonoBugs
{
	public class BrokenGenericCast
	{		
		public static Converter<TSource,TDest> GetUpcaster<TSource, TDest>() where TSource : TDest
		{
			return delegate(TSource obj) { return obj; };
		}

		public static Converter<TSource, TDest> GetDowncaster<TSource, TDest>() where TDest : TSource
		{
			return delegate(TSource obj) { return (TDest)obj; };
		}
		
		public static void Main ()
		{
		}
	}
}

