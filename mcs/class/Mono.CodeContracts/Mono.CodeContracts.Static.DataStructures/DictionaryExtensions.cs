using System;
using System.Collections.Generic;


namespace Mono.CodeContracts.Static.DataStructures
{
	static class DictionaryExtensions
	{
		public static void AddOrUpdate<K, V>(this IDictionary<K, List<V>> dict, K key, List<V> values)
	    {
	      List<V> list1;
	      if (dict.TryGetValue(key, out list1))
	      {
	        List<V> list2 = new List<V>((IEnumerable<V>) list1);
	        list2.AddRange((IEnumerable<V>) values);
	        dict[key] = list2;
	      }
	      else dict[key] = values;
	    }
	}
}

