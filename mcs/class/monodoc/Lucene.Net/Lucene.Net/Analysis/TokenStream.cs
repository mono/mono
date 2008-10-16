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
	
	/// <summary>A TokenStream enumerates the sequence of tokens, either from
	/// fields of a document or from query text.
	/// <p>
	/// This is an abstract class.  Concrete subclasses are:
	/// <ul>
	/// <li>{@link Tokenizer}, a TokenStream
	/// whose input is a Reader; and
	/// <li>{@link TokenFilter}, a TokenStream
	/// whose input is another TokenStream.
	/// </ul>
	/// </summary>
	
	public abstract class TokenStream
	{
		/// <summary>Returns the next token in the stream, or null at EOS. </summary>
		public abstract Token Next();
		
		/// <summary>Releases resources associated with this stream. </summary>
		public virtual void  Close()
		{
		}
	}
}