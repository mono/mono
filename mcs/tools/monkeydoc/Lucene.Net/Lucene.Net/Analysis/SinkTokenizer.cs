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
	
	
	/// <summary> A SinkTokenizer can be used to cache Tokens for use in an Analyzer
	/// <p/>
	/// WARNING: {@link TeeTokenFilter} and {@link SinkTokenizer} only work with the old TokenStream API.
	/// If you switch to the new API, you need to use {@link TeeSinkTokenFilter} instead, which offers 
	/// the same functionality.
	/// </summary>
	/// <seealso cref="TeeTokenFilter">
	/// </seealso>
	/// <deprecated> Use {@link TeeSinkTokenFilter} instead
	/// 
	/// 
	/// </deprecated>
    [Obsolete("Use TeeSinkTokenFilter instead")]
	public class SinkTokenizer:Tokenizer
	{
		protected internal System.Collections.IList lst = new System.Collections.ArrayList();
		protected internal System.Collections.IEnumerator iter;
		
		public SinkTokenizer(System.Collections.IList input)
		{
			this.lst = input;
			if (this.lst == null)
				this.lst = new System.Collections.ArrayList();
		}
		
		public SinkTokenizer()
		{
			this.lst = new System.Collections.ArrayList();
		}
		
		public SinkTokenizer(int initCap)
		{
			this.lst = new System.Collections.ArrayList(initCap);
		}
		
		/// <summary> Get the tokens in the internal List.
		/// <p/>
		/// WARNING: Adding tokens to this list requires the {@link #Reset()} method to be called in order for them
		/// to be made available.  Also, this Tokenizer does nothing to protect against {@link java.util.ConcurrentModificationException}s
		/// in the case of adds happening while {@link #Next(Mono.Lucene.Net.Analysis.Token)} is being called.
		/// <p/>
		/// WARNING: Since this SinkTokenizer can be reset and the cached tokens made available again, do not modify them. Modify clones instead.
		/// 
		/// </summary>
		/// <returns> A List of {@link Mono.Lucene.Net.Analysis.Token}s
		/// </returns>
		public virtual System.Collections.IList GetTokens()
		{
			return lst;
		}
		
		/// <summary> Returns the next token out of the list of cached tokens</summary>
		/// <returns> The next {@link Mono.Lucene.Net.Analysis.Token} in the Sink.
		/// </returns>
		/// <throws>  IOException </throws>
        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Analysis.TokenStream.Next(Mono.Lucene.Net.Analysis.Token)")]
		public override Token Next(Token reusableToken)
		{
			System.Diagnostics.Debug.Assert(reusableToken != null);
			if (iter == null)
				iter = lst.GetEnumerator();
			// Since this TokenStream can be reset we have to maintain the tokens as immutable
			if (iter.MoveNext())
			{
				Token nextToken = (Token) iter.Current;
				return (Token) nextToken.Clone();
			}
			return null;
		}
		
		/// <summary> Override this method to cache only certain tokens, or new tokens based
		/// on the old tokens.
		/// 
		/// </summary>
		/// <param name="t">The {@link Mono.Lucene.Net.Analysis.Token} to add to the sink
		/// </param>
		public virtual void  Add(Token t)
		{
			if (t == null)
				return ;
			lst.Add((Token) t.Clone());
		}
		
		public override void  Close()
		{
			//nothing to close
			input = null;
			lst = null;
		}
		
		/// <summary> Reset the internal data structures to the start at the front of the list of tokens.  Should be called
		/// if tokens were added to the list after an invocation of {@link #Next(Token)}
		/// </summary>
		/// <throws>  IOException </throws>
		public override void  Reset()
		{
			iter = lst.GetEnumerator();
		}
	}
}
