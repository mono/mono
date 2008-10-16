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
	
	/// <summary> TermPositions provides an interface for enumerating the &lt;document,
	/// frequency, &lt;position&gt;* &gt; tuples for a term.  <p> The document and
	/// frequency are the same as for a TermDocs.  The positions portion lists the ordinal
	/// positions of each occurrence of a term in a document.
	/// 
	/// </summary>
	/// <seealso cref="Monodoc.Lucene.Net.Index.IndexReader#termPositions">
	/// </seealso>
	
	public interface TermPositions:TermDocs
	{
		/// <summary>Returns next position in the current document.  It is an error to call
		/// this more than {@link #Freq()} times
		/// without calling {@link #Next()}<p> This is
		/// invalid until {@link #Next()} is called for
		/// the first time.
		/// </summary>
		int NextPosition();
	}
}