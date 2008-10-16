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
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary>TermDocs provides an interface for enumerating &lt;document, frequency&gt;
	/// pairs for a term.  <p> The document portion names each document containing
	/// the term.  Documents are indicated by number.  The frequency portion gives
	/// the number of times the term occurred in each document.  <p> The pairs are
	/// ordered by document number.
	/// </summary>
	/// <seealso cref="Monodoc.Lucene.Net.Index.IndexReader#termDocs">
	/// </seealso>
	
	public interface TermDocs
	{
		/// <summary>Sets this to the data for a term.
		/// The enumeration is reset to the start of the data for this term.
		/// </summary>
		void  Seek(Term term);
		
		/// <summary>Sets this to the data for the current term in a {@link TermEnum}.
		/// This may be optimized in some implementations.
		/// </summary>
		void  Seek(TermEnum termEnum);
		
		/// <summary>Returns the current document number.  <p> This is invalid until {@link
		/// #Next()} is called for the first time.
		/// </summary>
		int Doc();
		
		/// <summary>Returns the frequency of the term within the current document.  <p> This
		/// is invalid until {@link #Next()} is called for the first time.
		/// </summary>
		int Freq();
		
		/// <summary>Moves to the next pair in the enumeration.  <p> Returns true iff there is
		/// such a next pair in the enumeration. 
		/// </summary>
		bool Next();
		
		/// <summary>Attempts to read multiple entries from the enumeration, up to length of
		/// <i>docs</i>.  Document numbers are stored in <i>docs</i>, and term
		/// frequencies are stored in <i>freqs</i>.  The <i>freqs</i> array must be as
		/// long as the <i>docs</i> array.
		/// 
		/// <p>Returns the number of entries read.  Zero is only returned when the
		/// stream has been exhausted.  
		/// </summary>
		int Read(int[] docs, int[] freqs);
		
		/// <summary>Skips entries to the first beyond the current whose document number is
		/// greater than or equal to <i>target</i>. <p>Returns true iff there is such
		/// an entry.  <p>Behaves as if written: <pre>
		/// boolean skipTo(int target) {
		/// do {
		/// if (!next())
		/// return false;
		/// } while (target > doc());
		/// return true;
		/// }
		/// </pre>
		/// Some implementations are considerably more efficient than that.
		/// </summary>
		bool SkipTo(int target);
		
		/// <summary>Frees associated resources. </summary>
		void  Close();
	}
}