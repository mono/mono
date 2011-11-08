/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> Helper class for keeping Listss of Objects associated with keys. <b>WARNING: THIS CLASS IS NOT THREAD SAFE</b></summary>
    public class MapOfSets<T, V>
    {
		
		// TODO: This will be a HashSet<T> when we start using .NET Framework 3.5
		private System.Collections.Generic.IDictionary<T, System.Collections.Generic.Dictionary<V, V>> theMap;
		
		/// <param name="m">the backing store for this object
		/// </param>
		public MapOfSets(System.Collections.Generic.IDictionary<T, System.Collections.Generic.Dictionary<V, V>> m)
		{
			theMap = m;
		}
		
		/// <returns> direct access to the map backing this object.
		/// </returns>
		public virtual System.Collections.Generic.IDictionary<T, System.Collections.Generic.Dictionary<V, V>> GetMap()
		{
			return theMap;
		}
		
		/// <summary> Adds val to the Set associated with key in the Map.  If key is not 
		/// already in the map, a new Set will first be created.
		/// </summary>
		/// <returns> the size of the Set associated with key once val is added to it.
		/// </returns>
		public virtual int Put(T key, V val)
		{
            // TODO: This will be a HashSet<T> when we start using .NET Framework 3.5
            System.Collections.Generic.Dictionary<V, V> theSet;
            if (!theMap.TryGetValue(key, out theSet))
            {
                theSet = new System.Collections.Generic.Dictionary<V, V>(23);
                theMap[key] = theSet;
            }
            if (!theSet.ContainsKey(val))
            {
                theSet.Add(val, val);
            }
			return theSet.Count;
		}
		/// <summary> Adds multiple vals to the Set associated with key in the Map.  
		/// If key is not 
		/// already in the map, a new Set will first be created.
		/// </summary>
		/// <returns> the size of the Set associated with key once val is added to it.
		/// </returns>
		public virtual int PutAll(T key, System.Collections.Generic.Dictionary<V, V> vals)
		{
            // TODO: This will be a HashSet<T> when we start using .NET Framework 3.5
            System.Collections.Generic.Dictionary<V, V> theSet;
            if (!theMap.TryGetValue(key, out theSet))
            {
                theSet = new System.Collections.Generic.Dictionary<V, V>(23);
                theMap[key] = theSet;
            }
            foreach(V item in vals.Keys)
            {
                if (!theSet.ContainsKey(item))
                {
                    theSet.Add(item, item);
                }
            }
			return theSet.Count;
		}
	}
}
