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

using IndexReader = Mono.Lucene.Net.Index.IndexReader;

namespace Mono.Lucene.Net.Search.Function
{
	
	/// <summary> Expert: source of values for basic function queries.
	/// <p/>At its default/simplest form, values - one per doc - are used as the score of that doc.
	/// <p/>Values are instantiated as 
	/// {@link Mono.Lucene.Net.Search.Function.DocValues DocValues} for a particular reader.
	/// <p/>ValueSource implementations differ in RAM requirements: it would always be a factor
	/// of the number of documents, but for each document the number of bytes can be 1, 2, 4, or 8. 
	/// 
	/// <p/><font color="#FF0000">
	/// WARNING: The status of the <b>Search.Function</b> package is experimental. 
	/// The APIs introduced here might change in the future and will not be 
	/// supported anymore in such a case.</font>
	/// 
	/// 
	/// </summary>
	[Serializable]
	public abstract class ValueSource
	{
		
		/// <summary> Return the DocValues used by the function query.</summary>
		/// <param name="reader">the IndexReader used to read these values.
		/// If any caching is involved, that caching would also be IndexReader based.  
		/// </param>
		/// <throws>  IOException for any error. </throws>
		public abstract DocValues GetValues(IndexReader reader);
		
		/// <summary> description of field, used in explain() </summary>
		public abstract System.String Description();
		
		/* (non-Javadoc) @see java.lang.Object#toString() */
		public override System.String ToString()
		{
			return Description();
		}
		
		/// <summary> Needed for possible caching of query results - used by {@link ValueSourceQuery#equals(Object)}.</summary>
		/// <seealso cref="Object.equals(Object)">
		/// </seealso>
		abstract public  override bool Equals(System.Object o);
		
		/// <summary> Needed for possible caching of query results - used by {@link ValueSourceQuery#hashCode()}.</summary>
		/// <seealso cref="Object.hashCode()">
		/// </seealso>
		abstract public override int GetHashCode();
	}
}
