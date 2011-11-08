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

using Fieldable = Mono.Lucene.Net.Documents.Fieldable;
using AlreadyClosedException = Mono.Lucene.Net.Store.AlreadyClosedException;
using CloseableThreadLocal = Mono.Lucene.Net.Util.CloseableThreadLocal;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary>An Analyzer builds TokenStreams, which analyze text.  It thus represents a
	/// policy for extracting index terms from text.
	/// <p/>
	/// Typical implementations first build a Tokenizer, which breaks the stream of
	/// characters from the Reader into raw Tokens.  One or more TokenFilters may
	/// then be applied to the output of the Tokenizer.
	/// </summary>
	public abstract class Analyzer
	{
		/// <summary>Creates a TokenStream which tokenizes all the text in the provided
		/// Reader.  Must be able to handle null field name for
		/// backward compatibility.
		/// </summary>
		public abstract TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader);
		
		/// <summary>Creates a TokenStream that is allowed to be re-used
		/// from the previous time that the same thread called
		/// this method.  Callers that do not need to use more
		/// than one TokenStream at the same time from this
		/// analyzer should use this method for better
		/// performance.
		/// </summary>
		public virtual TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			return TokenStream(fieldName, reader);
		}
		
		private CloseableThreadLocal tokenStreams = new CloseableThreadLocal();
		
		/// <summary>Used by Analyzers that implement reusableTokenStream
		/// to retrieve previously saved TokenStreams for re-use
		/// by the same thread. 
		/// </summary>
		protected internal virtual System.Object GetPreviousTokenStream()
		{
			try
			{
				return tokenStreams.Get();
			}
			catch (System.NullReferenceException npe)
			{
				if (tokenStreams == null)
				{
					throw new AlreadyClosedException("this Analyzer is closed");
				}
				else
				{
					throw npe;
				}
			}
		}
		
		/// <summary>Used by Analyzers that implement reusableTokenStream
		/// to save a TokenStream for later re-use by the same
		/// thread. 
		/// </summary>
		protected internal virtual void  SetPreviousTokenStream(System.Object obj)
		{
			try
			{
				tokenStreams.Set(obj);
			}
			catch (System.NullReferenceException npe)
			{
				if (tokenStreams == null)
				{
					throw new AlreadyClosedException("this Analyzer is closed");
				}
				else
				{
					throw npe;
				}
			}
		}
		
		protected internal bool overridesTokenStreamMethod;
		
		/// <deprecated> This is only present to preserve
		/// back-compat of classes that subclass a core analyzer
		/// and override tokenStream but not reusableTokenStream 
		/// </deprecated>
        [Obsolete("This is only present to preserve back-compat of classes that subclass a core analyzer and override tokenStream but not reusableTokenStream ")]
		protected internal virtual void  SetOverridesTokenStreamMethod(System.Type baseClass)
		{
			
			System.Type[] params_Renamed = new System.Type[2];
			params_Renamed[0] = typeof(System.String);
			params_Renamed[1] = typeof(System.IO.TextReader);
			
			try
			{
				System.Reflection.MethodInfo m = this.GetType().GetMethod("TokenStream", (params_Renamed == null)?new System.Type[0]:(System.Type[]) params_Renamed);
				if (m != null)
				{
					overridesTokenStreamMethod = m.DeclaringType != baseClass;
				}
				else
				{
					overridesTokenStreamMethod = false;
				}
			}
			catch (System.MethodAccessException nsme)
			{
				overridesTokenStreamMethod = false;
			}
		}
		
		
		/// <summary> Invoked before indexing a Fieldable instance if
		/// terms have already been added to that field.  This allows custom
		/// analyzers to place an automatic position increment gap between
		/// Fieldable instances using the same field name.  The default value
		/// position increment gap is 0.  With a 0 position increment gap and
		/// the typical default token position increment of 1, all terms in a field,
		/// including across Fieldable instances, are in successive positions, allowing
		/// exact PhraseQuery matches, for instance, across Fieldable instance boundaries.
		/// 
		/// </summary>
		/// <param name="fieldName">Fieldable name being indexed.
		/// </param>
		/// <returns> position increment gap, added to the next token emitted from {@link #TokenStream(String,Reader)}
		/// </returns>
		public virtual int GetPositionIncrementGap(System.String fieldName)
		{
			return 0;
		}
		
		/// <summary> Just like {@link #getPositionIncrementGap}, except for
		/// Token offsets instead.  By default this returns 1 for
		/// tokenized fields and, as if the fields were joined
		/// with an extra space character, and 0 for un-tokenized
		/// fields.  This method is only called if the field
		/// produced at least one token for indexing.
		/// 
		/// </summary>
		/// <param name="field">the field just indexed
		/// </param>
		/// <returns> offset gap, added to the next token emitted from {@link #TokenStream(String,Reader)}
		/// </returns>
		public virtual int GetOffsetGap(Fieldable field)
		{
			if (field.IsTokenized())
				return 1;
			else
				return 0;
		}
		
		/// <summary>Frees persistent resources used by this Analyzer </summary>
		public virtual void  Close()
		{
			tokenStreams.Close();
			tokenStreams = null;
		}
	}
}
