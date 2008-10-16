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
	
	/// <summary>A Tokenizer is a TokenStream whose input is a Reader.
	/// <p>
	/// This is an abstract class.
	/// </summary>
	
	public abstract class Tokenizer : TokenStream
	{
		/// <summary>The text source for this Tokenizer. </summary>
		protected internal System.IO.TextReader input;
		
		/// <summary>Construct a tokenizer with null input. </summary>
		protected internal Tokenizer()
		{
		}
		
		/// <summary>Construct a token stream processing the given input. </summary>
		protected internal Tokenizer(System.IO.TextReader input)
		{
			this.input = input;
		}
		
		/// <summary>By default, closes the input Reader. </summary>
		public override void  Close()
		{
			input.Close();
		}
	}
}
