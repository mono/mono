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
	
	/// <summary> Returns primitive memory sizes for estimating RAM usage.
	/// 
	/// </summary>
	public abstract class MemoryModel
	{
		
		/// <returns> size of array beyond contents
		/// </returns>
		public abstract int GetArraySize();
		
		/// <returns> Class size overhead
		/// </returns>
		public abstract int GetClassSize();
		
		/// <param name="clazz">a primitive Class - bool, byte, char, short, long, float,
		/// short, double, int
		/// </param>
		/// <returns> the size in bytes of given primitive Class
		/// </returns>
		public abstract int GetPrimitiveSize(System.Type clazz);
		
		/// <returns> size of reference
		/// </returns>
		public abstract int GetReferenceSize();
	}
}
