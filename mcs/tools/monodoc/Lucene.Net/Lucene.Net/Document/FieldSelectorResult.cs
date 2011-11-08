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

using System.Runtime.InteropServices;

namespace Mono.Lucene.Net.Documents
{
	/// <summary>  Provides information about what should be done with this Field 
	/// 
	/// 
	/// </summary>
	//Replace with an enumerated type in 1.5
	[Serializable]
	public sealed class FieldSelectorResult
	{
		
		/// <summary> Load this {@link Field} every time the {@link Document} is loaded, reading in the data as it is encountered.
		/// {@link Document#GetField(String)} and {@link Document#GetFieldable(String)} should not return null.
		/// <p/>
		/// {@link Document#Add(Fieldable)} should be called by the Reader.
		/// </summary>
		[NonSerialized]
		public static readonly FieldSelectorResult LOAD = new FieldSelectorResult(0);
		/// <summary> Lazily load this {@link Field}.  This means the {@link Field} is valid, but it may not actually contain its data until
		/// invoked.  {@link Document#GetField(String)} SHOULD NOT BE USED.  {@link Document#GetFieldable(String)} is safe to use and should
		/// return a valid instance of a {@link Fieldable}.
		/// <p/>
		/// {@link Document#Add(Fieldable)} should be called by the Reader.
		/// </summary>
		[NonSerialized]
		public static readonly FieldSelectorResult LAZY_LOAD = new FieldSelectorResult(1);
		/// <summary> Do not load the {@link Field}.  {@link Document#GetField(String)} and {@link Document#GetFieldable(String)} should return null.
		/// {@link Document#Add(Fieldable)} is not called.
		/// <p/>
		/// {@link Document#Add(Fieldable)} should not be called by the Reader.
		/// </summary>
		[NonSerialized]
		public static readonly FieldSelectorResult NO_LOAD = new FieldSelectorResult(2);
		/// <summary> Load this field as in the {@link #LOAD} case, but immediately return from {@link Field} loading for the {@link Document}.  Thus, the
		/// Document may not have its complete set of Fields.  {@link Document#GetField(String)} and {@link Document#GetFieldable(String)} should
		/// both be valid for this {@link Field}
		/// <p/>
		/// {@link Document#Add(Fieldable)} should be called by the Reader.
		/// </summary>
		[NonSerialized]
		public static readonly FieldSelectorResult LOAD_AND_BREAK = new FieldSelectorResult(3);
		/// <summary> Behaves much like {@link #LOAD} but does not uncompress any compressed data.  This is used for internal purposes.
		/// {@link Document#GetField(String)} and {@link Document#GetFieldable(String)} should not return null.
		/// <p/>
		/// {@link Document#Add(Fieldable)} should be called by
		/// the Reader.
		/// </summary>
		/// <deprecated> This is an internal option only, and is
		/// no longer needed now that {@link CompressionTools}
		/// is used for field compression.
		/// </deprecated>
        [Obsolete("This is an internal option only, and is no longer needed now that CompressionTools is used for field compression.")]
		[NonSerialized]
		public static readonly FieldSelectorResult LOAD_FOR_MERGE = new FieldSelectorResult(4);
		
		/// <summary>Expert:  Load the size of this {@link Field} rather than its value.
		/// Size is measured as number of bytes required to store the field == bytes for a binary or any compressed value, and 2*chars for a String value.
		/// The size is stored as a binary value, represented as an int in a byte[], with the higher order byte first in [0]
		/// </summary>
		[NonSerialized]
		public static readonly FieldSelectorResult SIZE = new FieldSelectorResult(5);
		
		/// <summary>Expert: Like {@link #SIZE} but immediately break from the field loading loop, i.e., stop loading further fields, after the size is loaded </summary>
		[NonSerialized]
		public static readonly FieldSelectorResult SIZE_AND_BREAK = new FieldSelectorResult(6);
		
		
		
		private int id;
		
		private FieldSelectorResult(int id)
		{
			this.id = id;
		}
		
		public  override bool Equals(System.Object o)
		{
			if (this == o)
				return true;
			if (o == null || GetType() != o.GetType())
				return false;
			
			FieldSelectorResult that = (FieldSelectorResult) o;
			
			if (id != that.id)
				return false;
			
			return true;
		}
		
		public override int GetHashCode()
		{
			return id;
		}
	}
}
