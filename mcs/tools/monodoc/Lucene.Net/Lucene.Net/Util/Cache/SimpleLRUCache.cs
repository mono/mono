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
using System.Collections.Generic;

namespace Mono.Lucene.Net.Util.Cache
{
    public class SimpleLRUCache : SimpleMapCache
    {
        /// <summary>
        /// The maximum number of items to cache.
        /// </summary>
        private int capacity;

        /// <summary>
        /// The list to efficiently maintain the LRU state.
        /// </summary>
        private LinkedList<ListValueEntry> list;

        /// <summary>
        /// The dictionary to hash into any location in the list.
        /// </summary>
        private Dictionary<object, LinkedListNode<ListValueEntry>> lookup;

        /// <summary>
        /// The node instance to use/re-use when adding an item to the cache.
        /// </summary>
        private LinkedListNode<ListValueEntry> openNode;

        public SimpleLRUCache(int Capacity)
        {
            this.capacity = Capacity;
            this.list = new LinkedList<ListValueEntry>();
            this.lookup = new Dictionary<object, LinkedListNode<ListValueEntry>>(Capacity + 1);
            this.openNode = new LinkedListNode<ListValueEntry>(new ListValueEntry(null, null));
        }

        public override void Put(object Key, object Value)
        {
            if (Get(Key) == null)
            {
                this.openNode.Value.ItemKey = Key;
                this.openNode.Value.ItemValue = Value;
                this.list.AddFirst(this.openNode);
                this.lookup.Add(Key, this.openNode);

                if (this.list.Count > this.capacity)
                {
                    // last node is to be removed and saved for the next addition to the cache
                    this.openNode = this.list.Last;

                    // remove from list & dictionary
                    this.list.RemoveLast();
                    this.lookup.Remove(this.openNode.Value.ItemKey);
                }
                else
                {
                    // still filling the cache, create a new open node for the next time
                    this.openNode = new LinkedListNode<ListValueEntry>(new ListValueEntry(null, null));
                }
            }
        }

        public override object Get(object Key)
        {
            LinkedListNode<ListValueEntry> node = null;
            if(!this.lookup.TryGetValue(Key, out node))
            {
                return null;
            }
            this.list.Remove(node);
            this.list.AddFirst(node);
            return node.Value.ItemValue;
        }

        /// <summary>
        /// Container to hold the key and value to aid in removal from 
        /// the <see cref="lookup"/> dictionary when an item is removed from cache.
        /// </summary>
        class ListValueEntry
        {
            internal object ItemValue;
            internal object ItemKey;

            internal ListValueEntry(object key, object value)
            {
                this.ItemKey = key;
                this.ItemValue = value;
            }
        }
    }


#region NOT_USED_FROM_JLCA_PORT
/*
  
 //
 // This is the oringal port as it was generated via JLCA.
 // This code is not used.  It's here for referance only.
 //
  

	/// <summary> Simple LRU cache implementation that uses a LinkedHashMap.
	/// This cache is not synchronized, use {@link Cache#SynchronizedCache(Cache)}
	/// if needed.
	/// 
	/// </summary>
	public class SimpleLRUCache:SimpleMapCache
	{
		private class AnonymousClassLinkedHashMap : LinkedHashMap
		{
			public AnonymousClassLinkedHashMap(SimpleLRUCache enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(SimpleLRUCache enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private SimpleLRUCache enclosingInstance;
			public SimpleLRUCache Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			protected internal virtual bool RemoveEldestEntry(System.Collections.DictionaryEntry eldest)
			{
				return size() > Enclosing_Instance.cacheSize;
			}
		}
		private const float LOADFACTOR = 0.75f;
		
		private int cacheSize;
		
		/// <summary> Creates a last-recently-used cache with the specified size. </summary>
		public SimpleLRUCache(int cacheSize):base(null)
		{
			this.cacheSize = cacheSize;
			int capacity = (int) System.Math.Ceiling(cacheSize / LOADFACTOR) + 1;
			
			base.map = new AnonymousClassLinkedHashMap(this, capacity, LOADFACTOR, true);
		}
	}
*/
#endregion

}
