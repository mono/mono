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

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> Holds a map of String input to String output, to be used
	/// with {@link MappingCharFilter}.
	/// </summary>
	public class NormalizeCharMap
	{
		
		//Map<Character, NormalizeMap> submap;
		internal System.Collections.IDictionary submap;
		internal System.String normStr;
		internal int diff;
		
		/// <summary>Records a replacement to be applied to the inputs
		/// stream.  Whenever <code>singleMatch</code> occurs in
		/// the input, it will be replaced with
		/// <code>replacement</code>.
		/// 
		/// </summary>
		/// <param name="singleMatch">input String to be replaced
		/// </param>
		/// <param name="replacement">output String
		/// </param>
		public virtual void  Add(System.String singleMatch, System.String replacement)
		{
			NormalizeCharMap currMap = this;
			for (int i = 0; i < singleMatch.Length; i++)
			{
				char c = singleMatch[i];
				if (currMap.submap == null)
				{
					currMap.submap = new System.Collections.Hashtable(1);
				}
				NormalizeCharMap map = (NormalizeCharMap) currMap.submap[CharacterCache.ValueOf(c)];
				if (map == null)
				{
					map = new NormalizeCharMap();
					currMap.submap[c] = map;
				}
				currMap = map;
			}
			if (currMap.normStr != null)
			{
				throw new System.SystemException("MappingCharFilter: there is already a mapping for " + singleMatch);
			}
			currMap.normStr = replacement;
			currMap.diff = singleMatch.Length - replacement.Length;
		}
	}
}
