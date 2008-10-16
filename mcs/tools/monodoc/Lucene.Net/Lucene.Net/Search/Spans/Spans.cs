/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
namespace Monodoc.Lucene.Net.Search.Spans
{
	
	/// <summary>Expert: an enumeration of span matches.  Used to implement span searching.
	/// Each span represents a range of term positions within a document.  Matches
	/// are enumerated in order, by increasing document number, within that by
	/// increasing start position and finally by increasing end position. 
	/// </summary>
	public interface Spans
	{
		/// <summary>Move to the next match, returning true iff any such exists. </summary>
		bool Next();
		
		/// <summary>Skips to the first match beyond the current, whose document number is
		/// greater than or equal to <i>target</i>. <p>Returns true iff there is such
		/// a match.  <p>Behaves as if written: <pre>
		/// boolean skipTo(int target) {
		/// do {
		/// if (!next())
		/// return false;
		/// } while (target > doc());
		/// return true;
		/// }
		/// </pre>
		/// Most implementations are considerably more efficient than that.
		/// </summary>
		bool SkipTo(int target);
		
		/// <summary>Returns the document number of the current match.  Initially invalid. </summary>
		int Doc();
		
		/// <summary>Returns the start position of the current match.  Initially invalid. </summary>
		int Start();
		
		/// <summary>Returns the end position of the current match.  Initially invalid. </summary>
		int End();
	}
}