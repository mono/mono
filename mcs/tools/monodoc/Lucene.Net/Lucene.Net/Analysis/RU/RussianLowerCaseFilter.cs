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
using Token = Monodoc.Lucene.Net.Analysis.Token;
using TokenFilter = Monodoc.Lucene.Net.Analysis.TokenFilter;
using TokenStream = Monodoc.Lucene.Net.Analysis.TokenStream;
namespace Monodoc.Lucene.Net.Analysis.RU
{
	
	/// <summary> Normalizes token text to lower case, analyzing given ("russian") charset.
	/// 
	/// </summary>
	/// <author>   Boris Okner, b.okner@rogers.com
	/// </author>
	/// <version>  $Id: RussianLowerCaseFilter.java,v 1.4 2004/03/29 22:48:01 cutting Exp $
	/// </version>
	public sealed class RussianLowerCaseFilter : TokenFilter
	{
		internal char[] charset;
		
		public RussianLowerCaseFilter(TokenStream in_Renamed, char[] charset):base(in_Renamed)
		{
			this.charset = charset;
		}
		
		public override Token Next()
		{
			Token t = input.Next();
			
			if (t == null)
				return null;
			
			System.String txt = t.TermText();
			
			char[] chArray = txt.ToCharArray();
			for (int i = 0; i < chArray.Length; i++)
			{
				chArray[i] = RussianCharsets.ToLowerCase(chArray[i], charset);
			}
			
			System.String newTxt = new System.String(chArray);
			// create new token
			Token newToken = new Token(newTxt, t.StartOffset(), t.EndOffset());
			
			return newToken;
		}
	}
}