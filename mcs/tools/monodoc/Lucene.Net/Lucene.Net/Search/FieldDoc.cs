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

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Expert: A ScoreDoc which also contains information about
	/// how to sort the referenced document.  In addition to the
	/// document number and score, this object contains an array
	/// of values for the document from the field(s) used to sort.
	/// For example, if the sort criteria was to sort by fields
	/// "a", "b" then "c", the <code>fields</code> object array
	/// will have three elements, corresponding respectively to
	/// the term values for the document in fields "a", "b" and "c".
	/// The class of each element in the array will be either
	/// Integer, Float or String depending on the type of values
	/// in the terms of each field.
	/// 
	/// <p/>Created: Feb 11, 2004 1:23:38 PM
	/// 
	/// </summary>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: FieldDoc.java 773194 2009-05-09 10:36:41Z mikemccand $
	/// </version>
	/// <seealso cref="ScoreDoc">
	/// </seealso>
	/// <seealso cref="TopFieldDocs">
	/// </seealso>
	[Serializable]
	public class FieldDoc:ScoreDoc
	{
		
		/// <summary>Expert: The values which are used to sort the referenced document.
		/// The order of these will match the original sort criteria given by a
		/// Sort object.  Each Object will be either an Integer, Float or String,
		/// depending on the type of values in the terms of the original field.
		/// </summary>
		/// <seealso cref="Sort">
		/// </seealso>
		/// <seealso cref="Searcher.Search(Query,Filter,int,Sort)">
		/// </seealso>
        [NonSerialized]
		public System.IComparable[] fields;
		
		/// <summary>Expert: Creates one of these objects with empty sort information. </summary>
		public FieldDoc(int doc, float score):base(doc, score)
		{
		}
		
		/// <summary>Expert: Creates one of these objects with the given sort information. </summary>
		public FieldDoc(int doc, float score, System.IComparable[] fields):base(doc, score)
		{
			this.fields = fields;
		}
		
		// A convenience method for debugging.
		public override System.String ToString()
		{
			// super.toString returns the doc and score information, so just add the
			// fields information
			System.Text.StringBuilder sb = new System.Text.StringBuilder(base.ToString());
			sb.Append("[");
			for (int i = 0; i < fields.Length; i++)
			{
				sb.Append(fields[i]).Append(", ");
			}
			sb.Length -= 2; // discard last ", "
			sb.Append("]");
			return sb.ToString();
		}

        #region SERIALIZATION
        internal object[] fieldsClone = null;

        [System.Runtime.Serialization.OnSerializing]
        void OnSerializing(System.Runtime.Serialization.StreamingContext context)
        {
            if (fields == null) return;

            // Copy "fields" to "fieldsClone"
            fieldsClone = new object[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                fieldsClone[i] = fields[i];
            }
        }

        [System.Runtime.Serialization.OnDeserialized]
        void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            if (fieldsClone == null) return;

            // Form "fields" from "fieldsClone"
            fields = new IComparable[fieldsClone.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = (IComparable)fieldsClone[i];
            }
        }
        #endregion
	}
}
