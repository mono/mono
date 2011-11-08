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
	
	/// <summary> Estimates the size of a given Object using a given MemoryModel for primitive
	/// size information.
	/// 
	/// Resource Usage: 
	/// 
	/// Internally uses a Map to temporally hold a reference to every
	/// object seen. 
	/// 
	/// If checkIntered, all Strings checked will be interned, but those
	/// that were not already interned will be released for GC when the
	/// estimate is complete.
	/// </summary>
	public sealed class RamUsageEstimator
	{
		private MemoryModel memoryModel;
		
		private System.Collections.IDictionary seen;
		
		private int refSize;
		private int arraySize;
		private int classSize;
		
		private bool checkInterned;
		
		/// <summary> Constructs this object with an AverageGuessMemoryModel and
		/// checkInterned = true.
		/// </summary>
		public RamUsageEstimator():this(new AverageGuessMemoryModel())
		{
		}
		
		/// <param name="checkInterned">check if Strings are interned and don't add to size
		/// if they are. Defaults to true but if you know the objects you are checking
		/// won't likely contain many interned Strings, it will be faster to turn off
		/// intern checking.
		/// </param>
		public RamUsageEstimator(bool checkInterned):this(new AverageGuessMemoryModel(), checkInterned)
		{
		}
		
		/// <param name="memoryModel">MemoryModel to use for primitive object sizes.
		/// </param>
		public RamUsageEstimator(MemoryModel memoryModel):this(memoryModel, true)
		{
		}
		
		/// <param name="memoryModel">MemoryModel to use for primitive object sizes.
		/// </param>
		/// <param name="checkInterned">check if Strings are interned and don't add to size
		/// if they are. Defaults to true but if you know the objects you are checking
		/// won't likely contain many interned Strings, it will be faster to turn off
		/// intern checking.
		/// </param>
		public RamUsageEstimator(MemoryModel memoryModel, bool checkInterned)
		{
			this.memoryModel = memoryModel;
			this.checkInterned = checkInterned;
			// Use Map rather than Set so that we can use an IdentityHashMap - not
			// seeing an IdentityHashSet
            seen = new System.Collections.Hashtable(64);    // {{Aroush-2.9}} Port issue; need to mimic java's IdentityHashMap equals() through C#'s Equals()
			this.refSize = memoryModel.GetReferenceSize();
			this.arraySize = memoryModel.GetArraySize();
			this.classSize = memoryModel.GetClassSize();
		}
		
		public long EstimateRamUsage(System.Object obj)
		{
			long size = Size(obj);
			seen.Clear();
			return size;
		}
		
		private long Size(System.Object obj)
		{
			if (obj == null)
			{
				return 0;
			}
			// interned not part of this object
			if (checkInterned && obj is System.String && obj == (System.Object) String.Intern(((System.String) obj)))
			{
				// interned string will be eligible
				// for GC on
				// estimateRamUsage(Object) return
				return 0;
			}
			
			// skip if we have seen before
			if (seen.Contains(obj))
			{
				return 0;
			}
			
			// add to seen
			seen[obj] = null;
			
			System.Type clazz = obj.GetType();
			if (clazz.IsArray)
			{
				return SizeOfArray(obj);
			}
			
			long size = 0;
			
			// walk type hierarchy
			while (clazz != null)
			{
				System.Reflection.FieldInfo[] fields = clazz.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static);
				for (int i = 0; i < fields.Length; i++)
				{
					if (fields[i].IsStatic)
					{
						continue;
					}
					
					if (fields[i].FieldType.IsPrimitive)
					{
						size += memoryModel.GetPrimitiveSize(fields[i].FieldType);
					}
					else
					{
						size += refSize;
                        fields[i].GetType(); 
						try
						{
							System.Object value_Renamed = fields[i].GetValue(obj);
							if (value_Renamed != null)
							{
								size += Size(value_Renamed);
							}
						}
						catch (System.UnauthorizedAccessException ex)
						{
							// ignore for now?
						}
					}
				}
				clazz = clazz.BaseType;
			}
			size += classSize;
			return size;
		}
		
		private long SizeOfArray(System.Object obj)
		{
			int len = ((System.Array) obj).Length;
			if (len == 0)
			{
				return 0;
			}
			long size = arraySize;
			System.Type arrayElementClazz = obj.GetType().GetElementType();
			if (arrayElementClazz.IsPrimitive)
			{
				size += len * memoryModel.GetPrimitiveSize(arrayElementClazz);
			}
			else
			{
				for (int i = 0; i < len; i++)
				{
					size += refSize + Size(((System.Array) obj).GetValue(i));
				}
			}
			
			return size;
		}
		
		private const long ONE_KB = 1024;
		private static readonly long ONE_MB = ONE_KB * ONE_KB;
		private static readonly long ONE_GB = ONE_KB * ONE_MB;
		
		/// <summary> Return good default units based on byte size.</summary>
		public static System.String HumanReadableUnits(long bytes, System.IFormatProvider df)
		{
			System.String newSizeAndUnits;
			
			if (bytes / ONE_GB > 0)
			{
				newSizeAndUnits = System.Convert.ToString(((float) bytes / ONE_GB), df) + " GB";
			}
			else if (bytes / ONE_MB > 0)
			{
				newSizeAndUnits = System.Convert.ToString((float) bytes / ONE_MB, df) + " MB";
			}
			else if (bytes / ONE_KB > 0)
			{
				newSizeAndUnits = System.Convert.ToString((float) bytes / ONE_KB, df) + " KB";
			}
			else
			{
				newSizeAndUnits = System.Convert.ToString(bytes) + " bytes";
			}
			
			return newSizeAndUnits;
		}
	}
}
