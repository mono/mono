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

namespace Mono.Lucene.Net.Documents
{
	/// <summary> Declare what fields to load normally and what fields to load lazily
	/// 
	/// 
	/// </summary>
	[Serializable]
	public class SetBasedFieldSelector : FieldSelector
	{
		
		private System.Collections.Hashtable fieldsToLoad;
		private System.Collections.Hashtable lazyFieldsToLoad;
		
		
		
		/// <summary> Pass in the Set of {@link Field} names to load and the Set of {@link Field} names to load lazily.  If both are null, the
		/// Document will not have any {@link Field} on it.  
		/// </summary>
		/// <param name="fieldsToLoad">A Set of {@link String} field names to load.  May be empty, but not null
		/// </param>
		/// <param name="lazyFieldsToLoad">A Set of {@link String} field names to load lazily.  May be empty, but not null  
		/// </param>
		public SetBasedFieldSelector(System.Collections.Hashtable fieldsToLoad, System.Collections.Hashtable lazyFieldsToLoad)
		{
			this.fieldsToLoad = fieldsToLoad;
			this.lazyFieldsToLoad = lazyFieldsToLoad;
		}
		
		/// <summary> Indicate whether to load the field with the given name or not. If the {@link Field#Name()} is not in either of the 
		/// initializing Sets, then {@link Mono.Lucene.Net.Documents.FieldSelectorResult#NO_LOAD} is returned.  If a Field name
		/// is in both <code>fieldsToLoad</code> and <code>lazyFieldsToLoad</code>, lazy has precedence.
		/// 
		/// </summary>
		/// <param name="fieldName">The {@link Field} name to check
		/// </param>
		/// <returns> The {@link FieldSelectorResult}
		/// </returns>
		public virtual FieldSelectorResult Accept(System.String fieldName)
		{
			FieldSelectorResult result = FieldSelectorResult.NO_LOAD;
			if (fieldsToLoad.Contains(fieldName) == true)
			{
				result = FieldSelectorResult.LOAD;
			}
			if (lazyFieldsToLoad.Contains(fieldName) == true)
			{
				result = FieldSelectorResult.LAZY_LOAD;
			}
			return result;
		}
	}
}
