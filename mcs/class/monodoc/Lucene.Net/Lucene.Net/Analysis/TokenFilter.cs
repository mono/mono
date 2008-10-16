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
	
	/// <summary>A TokenFilter is a TokenStream whose input is another token stream.
	/// <p>
	/// This is an abstract class.
	/// </summary>
	
	public abstract class TokenFilter : TokenStream
	{
		/// <summary>The source of tokens for this filter. </summary>
		protected internal TokenStream input;
		
		/// <summary>Call TokenFilter(TokenStream) instead.</summary>
		/// <deprecated> 
		/// </deprecated>
		protected internal TokenFilter()
		{
		}
		
		/// <summary>Construct a token stream filtering the given input. </summary>
		protected internal TokenFilter(TokenStream input)
		{
			this.input = input;
		}
		
		/// <summary>Close the input TokenStream. </summary>
		public override void  Close()
		{
			input.Close();
		}
	}
}