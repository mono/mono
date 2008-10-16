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
namespace Monodoc.Lucene.Net.Analysis
{
	
	/// <summary>An Analyzer builds TokenStreams, which analyze text.  It thus represents a
	/// policy for extracting index terms from text.
	/// <p>
	/// Typical implementations first build a Tokenizer, which breaks the stream of
	/// characters from the Reader into raw Tokens.  One or more TokenFilters may
	/// then be applied to the output of the Tokenizer.
	/// </p>
	/// <p>
	/// WARNING: You must override one of the methods defined by this class in your
	/// subclass or the Analyzer will enter an infinite loop.
	/// </p>
	/// </summary>
	public abstract class Analyzer
	{
		/// <summary>Creates a TokenStream which tokenizes all the text in the provided
		/// Reader.  Default implementation forwards to tokenStream(Reader) for 
		/// compatibility with older version.  Override to allow Analyzer to choose 
		/// strategy based on document and/or Field.  Must be able to handle null
		/// Field name for backward compatibility. 
		/// </summary>
		public virtual TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			// implemented for backward compatibility
			return TokenStream(reader);
		}
		
		/// <summary>Creates a TokenStream which tokenizes all the text in the provided
		/// Reader.  Provided for backward compatibility only.
		/// </summary>
		/// <deprecated> use TokenStream(String, Reader) instead.
		/// </deprecated>
		/// <seealso cref="Reader)">
		/// </seealso>
		public virtual TokenStream TokenStream(System.IO.TextReader reader)
		{
			return TokenStream(null, reader);
		}
	}
}