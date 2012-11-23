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

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> "Tokenizes" the entire stream as a single token. This is useful
	/// for data like zip codes, ids, and some product names.
	/// </summary>
	public class KeywordAnalyzer:Analyzer
	{
		public KeywordAnalyzer()
		{
			SetOverridesTokenStreamMethod(typeof(KeywordAnalyzer));
		}
		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			return new KeywordTokenizer(reader);
		}
		public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			if (overridesTokenStreamMethod)
			{
				// LUCENE-1678: force fallback to tokenStream() if we
				// have been subclassed and that subclass overrides
				// tokenStream but not reusableTokenStream
				return TokenStream(fieldName, reader);
			}
			Tokenizer tokenizer = (Tokenizer) GetPreviousTokenStream();
			if (tokenizer == null)
			{
				tokenizer = new KeywordTokenizer(reader);
				SetPreviousTokenStream(tokenizer);
			}
			else
				tokenizer.Reset(reader);
			return tokenizer;
		}
	}
}
