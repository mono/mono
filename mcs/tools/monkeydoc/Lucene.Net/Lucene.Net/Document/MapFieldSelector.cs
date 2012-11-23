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
	
	/// <summary> A {@link FieldSelector} based on a Map of field names to {@link FieldSelectorResult}s
	/// 
	/// </summary>
	[Serializable]
	public class MapFieldSelector : FieldSelector
	{
		
		internal System.Collections.IDictionary fieldSelections;
		
		/// <summary>Create a a MapFieldSelector</summary>
		/// <param name="fieldSelections">maps from field names (String) to {@link FieldSelectorResult}s
		/// </param>
		public MapFieldSelector(System.Collections.IDictionary fieldSelections)
		{
			this.fieldSelections = fieldSelections;
		}
		
		/// <summary>Create a a MapFieldSelector</summary>
		/// <param name="fields">fields to LOAD.  List of Strings.  All other fields are NO_LOAD.
		/// </param>
		public MapFieldSelector(System.Collections.IList fields)
		{
			fieldSelections = new System.Collections.Hashtable(fields.Count * 5 / 3);
			for (int i = 0; i < fields.Count; i++)
				fieldSelections[fields[i]] = FieldSelectorResult.LOAD;
		}
		
		/// <summary>Create a a MapFieldSelector</summary>
		/// <param name="fields">fields to LOAD.  All other fields are NO_LOAD.
		/// </param>
		public MapFieldSelector(System.String[] fields)
		{
			fieldSelections = new System.Collections.Hashtable(fields.Length * 5 / 3);
			for (int i = 0; i < fields.Length; i++)
				fieldSelections[fields[i]] = FieldSelectorResult.LOAD;
		}
		
		/// <summary>Load field according to its associated value in fieldSelections</summary>
		/// <param name="field">a field name
		/// </param>
		/// <returns> the fieldSelections value that field maps to or NO_LOAD if none.
		/// </returns>
		public virtual FieldSelectorResult Accept(System.String field)
		{
			FieldSelectorResult selection = (FieldSelectorResult) fieldSelections[field];
			return selection != null?selection:FieldSelectorResult.NO_LOAD;
		}
	}
}
