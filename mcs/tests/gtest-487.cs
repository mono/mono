using System;
using System.Collections.Generic;

namespace Limada.MonoTests.Generics
{
	public class MultiDictionary<K, V, TDictionary>
		where TDictionary : IDictionary<K, ICollection<V>>
	{ }

	public class Test
	{
		public static void Main ()
		{
			var c = new MultiDictionary<int, int, Dictionary<int, ICollection<int>>> ();
		}
	}
}
