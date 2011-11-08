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

namespace Mono.Lucene.Net.Util.Cache
{
	
	/// <summary> Simple cache implementation that uses a HashMap to store (key, value) pairs.
	/// This cache is not synchronized, use {@link Cache#SynchronizedCache(Cache)}
	/// if needed.
	/// </summary>
	public class SimpleMapCache:Cache
	{
		internal System.Collections.IDictionary map;
		
		public SimpleMapCache():this(new System.Collections.Hashtable())
		{
		}
		
		public SimpleMapCache(System.Collections.IDictionary map)
		{
			this.map = map;
		}
		
		public override System.Object Get(System.Object key)
		{
			return map[key];
		}
		
		public override void  Put(System.Object key, System.Object value_Renamed)
		{
			map[key] = value_Renamed;
		}
		
		public override void  Close()
		{
			// NOOP
		}
		
		public override bool ContainsKey(System.Object key)
		{
			return map.Contains(key);
		}
		
		/// <summary> Returns a Set containing all keys in this cache.</summary>
		public virtual System.Collections.ICollection KeySet()
		{
			return map.Keys;
		}
		
		internal override Cache GetSynchronizedCache()
		{
			return new SynchronizedSimpleMapCache(this);
		}
		
		private class SynchronizedSimpleMapCache:SimpleMapCache
		{
			internal System.Object mutex;
			internal SimpleMapCache cache;
			
			internal SynchronizedSimpleMapCache(SimpleMapCache cache)
			{
				this.cache = cache;
				this.mutex = this;
			}
			
			public override void  Put(System.Object key, System.Object value_Renamed)
			{
				lock (mutex)
				{
					cache.Put(key, value_Renamed);
				}
			}
			
			public override System.Object Get(System.Object key)
			{
				lock (mutex)
				{
					return cache.Get(key);
				}
			}
			
			public override bool ContainsKey(System.Object key)
			{
				lock (mutex)
				{
					return cache.ContainsKey(key);
				}
			}
			
			public override void  Close()
			{
				lock (mutex)
				{
					cache.Close();
				}
			}
			
			public override System.Collections.ICollection KeySet()
			{
				lock (mutex)
				{
					return cache.KeySet();
				}
			}
			
			internal override Cache GetSynchronizedCache()
			{
				return this;
			}
		}
	}
}
