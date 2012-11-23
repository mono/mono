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
	
	/// <summary>Expert: Describes the score computation for document and query, and
	/// can distinguish a match independent of a positive value. 
	/// </summary>
	[Serializable]
	public class ComplexExplanation:Explanation
	{
		private System.Boolean? match;
        private bool isMatchSet = false;
		
		public ComplexExplanation():base()
		{
		}
		
		public ComplexExplanation(bool match, float value_Renamed, System.String description):base(value_Renamed, description)
		{
			this.match = match;
            this.isMatchSet = true;
		}
		
		/// <summary> The match status of this explanation node.</summary>
		/// <returns> May be null if match status is unknown
		/// </returns>
		public virtual System.Boolean? GetMatch()
		{
			return match;
		}
		/// <summary> Sets the match status assigned to this explanation node.</summary>
		/// <param name="match">May be null if match status is unknown
		/// </param>
		public virtual void  SetMatch(System.Boolean? match)
		{
			this.match = match;
            this.isMatchSet = true;
		}
		/// <summary> Indicates whether or not this Explanation models a good match.
		/// 
		/// <p/>
		/// If the match status is explicitly set (i.e.: not null) this method
		/// uses it; otherwise it defers to the superclass.
		/// <p/>
		/// </summary>
		/// <seealso cref="getMatch">
		/// </seealso>
		public override bool IsMatch()
		{
			System.Boolean? m = GetMatch();
            return m ?? base.IsMatch();
		}
		
		protected internal override System.String GetSummary()
		{
            if (isMatchSet == false)
				return base.GetSummary();
			
			return GetValue() + " = " + (IsMatch()?"(MATCH) ":"(NON-MATCH) ") + GetDescription();
		}
	}
}
