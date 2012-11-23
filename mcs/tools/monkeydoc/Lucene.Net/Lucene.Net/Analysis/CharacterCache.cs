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
	
	/// <summary> Replacement for Java 1.5 Character.valueOf()</summary>
	/// <deprecated> Move to Character.valueOf() in 3.0
	/// </deprecated>
    [Obsolete("Move to Character.valueOf() in 3.0")]
	public class CharacterCache
	{
		
		private static readonly System.Char[] cache = new System.Char[128];
		
		/// <summary> Returns a Character instance representing the given char value
		/// 
		/// </summary>
		/// <param name="c">a char value
		/// </param>
		/// <returns> a Character representation of the given char value.
		/// </returns>
		public static System.Char ValueOf(char c)
		{
			if (c < cache.Length)
			{
				return cache[(int) c];
			}
			return c;
		}
		static CharacterCache()
		{
			{
				for (int i = 0; i < cache.Length; i++)
				{
					cache[i] = (char) i;
				}
			}
		}
	}
}
