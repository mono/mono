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

using Attribute = Mono.Lucene.Net.Util.Attribute;

namespace Mono.Lucene.Net.Analysis.Tokenattributes
{
	
	/// <summary>The positionIncrement determines the position of this token
	/// relative to the previous Token in a TokenStream, used in phrase
	/// searching.
	/// 
	/// <p/>The default value is one.
	/// 
	/// <p/>Some common uses for this are:<ul>
	/// 
	/// <li>Set it to zero to put multiple terms in the same position.  This is
	/// useful if, e.g., a word has multiple stems.  Searches for phrases
	/// including either stem will match.  In this case, all but the first stem's
	/// increment should be set to zero: the increment of the first instance
	/// should be one.  Repeating a token with an increment of zero can also be
	/// used to boost the scores of matches on that token.</li>
	/// 
	/// <li>Set it to values greater than one to inhibit exact phrase matches.
	/// If, for example, one does not want phrases to match across removed stop
	/// words, then one could build a stop word filter that removes stop words and
	/// also sets the increment to the number of stop words removed before each
	/// non-stop word.  Then exact phrase queries will only match when the terms
	/// occur with no intervening stop words.</li>
	/// 
	/// </ul>
	/// 
	/// </summary>
	/// <seealso cref="Mono.Lucene.Net.Index.TermPositions">
	/// </seealso>
	public interface PositionIncrementAttribute:Attribute
	{
		/// <summary>Set the position increment. The default value is one.
		/// 
		/// </summary>
		/// <param name="positionIncrement">the distance from the prior term
		/// </param>
		void  SetPositionIncrement(int positionIncrement);
		
		/// <summary>Returns the position increment of this Token.</summary>
		/// <seealso cref="setPositionIncrement">
		/// </seealso>
		int GetPositionIncrement();
	}
}
