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

using FlagsAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.FlagsAttribute;
using OffsetAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.OffsetAttribute;
using PayloadAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.PayloadAttribute;
using PositionIncrementAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.PositionIncrementAttribute;
using TermAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TermAttribute;
using TypeAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TypeAttribute;
using Document = Mono.Lucene.Net.Documents.Document;
using Field = Mono.Lucene.Net.Documents.Field;
using IndexWriter = Mono.Lucene.Net.Index.IndexWriter;
using Attribute = Mono.Lucene.Net.Util.Attribute;
using AttributeImpl = Mono.Lucene.Net.Util.AttributeImpl;
using AttributeSource = Mono.Lucene.Net.Util.AttributeSource;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> A <code>TokenStream</code> enumerates the sequence of tokens, either from
	/// {@link Field}s of a {@link Document} or from query text.
	/// <p/>
	/// This is an abstract class. Concrete subclasses are:
	/// <ul>
	/// <li>{@link Tokenizer}, a <code>TokenStream</code> whose input is a Reader; and</li>
	/// <li>{@link TokenFilter}, a <code>TokenStream</code> whose input is another
	/// <code>TokenStream</code>.</li>
	/// </ul>
	/// A new <code>TokenStream</code> API has been introduced with Lucene 2.9. This API
	/// has moved from being {@link Token} based to {@link Attribute} based. While
	/// {@link Token} still exists in 2.9 as a convenience class, the preferred way
	/// to store the information of a {@link Token} is to use {@link AttributeImpl}s.
	/// <p/>
	/// <code>TokenStream</code> now extends {@link AttributeSource}, which provides
	/// access to all of the token {@link Attribute}s for the <code>TokenStream</code>.
	/// Note that only one instance per {@link AttributeImpl} is created and reused
	/// for every token. This approach reduces object creation and allows local
	/// caching of references to the {@link AttributeImpl}s. See
	/// {@link #IncrementToken()} for further details.
	/// <p/>
	/// <b>The workflow of the new <code>TokenStream</code> API is as follows:</b>
	/// <ol>
	/// <li>Instantiation of <code>TokenStream</code>/{@link TokenFilter}s which add/get
	/// attributes to/from the {@link AttributeSource}.</li>
	/// <li>The consumer calls {@link TokenStream#Reset()}.</li>
	/// <li>The consumer retrieves attributes from the stream and stores local
	/// references to all attributes it wants to access</li>
	/// <li>The consumer calls {@link #IncrementToken()} until it returns false and
	/// consumes the attributes after each call.</li>
	/// <li>The consumer calls {@link #End()} so that any end-of-stream operations
	/// can be performed.</li>
	/// <li>The consumer calls {@link #Close()} to release any resource when finished
	/// using the <code>TokenStream</code></li>
	/// </ol>
	/// To make sure that filters and consumers know which attributes are available,
	/// the attributes must be added during instantiation. Filters and consumers are
	/// not required to check for availability of attributes in
	/// {@link #IncrementToken()}.
	/// <p/>
	/// You can find some example code for the new API in the analysis package level
	/// Javadoc.
	/// <p/>
	/// Sometimes it is desirable to capture a current state of a <code>TokenStream</code>
	/// , e. g. for buffering purposes (see {@link CachingTokenFilter},
	/// {@link TeeSinkTokenFilter}). For this usecase
	/// {@link AttributeSource#CaptureState} and {@link AttributeSource#RestoreState}
	/// can be used.
	/// </summary>
	public abstract class TokenStream:AttributeSource
	{
		private void  InitBlock()
		{
			supportedMethods = GetSupportedMethods(this.GetType());
		}
		
		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private static readonly AttributeFactory DEFAULT_TOKEN_WRAPPER_ATTRIBUTE_FACTORY = new TokenWrapperAttributeFactory(AttributeFactory.DEFAULT_ATTRIBUTE_FACTORY);
		
		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private TokenWrapper tokenWrapper;
		
		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private static bool onlyUseNewAPI = false;
		
		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private MethodSupport supportedMethods;
		
		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private sealed class MethodSupport
		{
			internal bool hasIncrementToken;
			internal bool hasReusableNext;
			internal bool hasNext;
			
			internal MethodSupport(System.Type clazz)
			{
				hasIncrementToken = IsMethodOverridden(clazz, "IncrementToken", METHOD_NO_PARAMS);
				hasReusableNext = IsMethodOverridden(clazz, "Next", METHOD_TOKEN_PARAM);
				hasNext = IsMethodOverridden(clazz, "Next", METHOD_NO_PARAMS);
			}
			
			private static bool IsMethodOverridden(System.Type clazz, System.String name, System.Type[] params_Renamed)
			{
				try
				{
					return clazz.GetMethod(name, params_Renamed).DeclaringType != typeof(TokenStream);
				}
				catch (System.MethodAccessException e)
				{
					// should not happen
					throw new System.SystemException(e.Message, e);
				}
			}
			
			private static readonly System.Type[] METHOD_NO_PARAMS = new System.Type[0];
			private static readonly System.Type[] METHOD_TOKEN_PARAM = new System.Type[]{typeof(Token)};
		}
		
		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private static readonly System.Collections.Hashtable knownMethodSupport = new System.Collections.Hashtable();

        // {{Aroush-2.9 Port issue, need to mimic java's IdentityHashMap
        /*
         * From Java docs:
         * This class implements the Map interface with a hash table, using 
         * reference-equality in place of object-equality when comparing keys 
         * (and values). In other words, in an IdentityHashMap, two keys k1 and k2 
         * are considered equal if and only if (k1==k2). (In normal Map 
         * implementations (like HashMap) two keys k1 and k2 are considered 
         * equal if and only if (k1==null ? k2==null : k1.equals(k2)).) 
         */
        // Aroush-2.9}}

		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private static MethodSupport GetSupportedMethods(System.Type clazz)
		{
			MethodSupport supportedMethods;
			lock (knownMethodSupport)
			{
				supportedMethods = (MethodSupport) knownMethodSupport[clazz];
				if (supportedMethods == null)
				{
					knownMethodSupport.Add(clazz, supportedMethods = new MethodSupport(clazz));
				}
			}
			return supportedMethods;
		}
		
		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private sealed class TokenWrapperAttributeFactory:AttributeFactory
		{
			private AttributeFactory delegate_Renamed;
			
			internal TokenWrapperAttributeFactory(AttributeFactory delegate_Renamed)
			{
				this.delegate_Renamed = delegate_Renamed;
			}
			
			public override AttributeImpl CreateAttributeInstance(System.Type attClass)
			{
				return attClass.IsAssignableFrom(typeof(TokenWrapper))?new TokenWrapper():delegate_Renamed.CreateAttributeInstance(attClass);
			}
			
			// this is needed for TeeSinkTokenStream's check for compatibility of AttributeSource,
			// so two TokenStreams using old API have the same AttributeFactory wrapped by this one.
			public  override bool Equals(System.Object other)
			{
				if (this == other)
					return true;
				if (other is TokenWrapperAttributeFactory)
				{
					TokenWrapperAttributeFactory af = (TokenWrapperAttributeFactory) other;
					return this.delegate_Renamed.Equals(af.delegate_Renamed);
				}
				return false;
			}
			
			public override int GetHashCode()
			{
				return delegate_Renamed.GetHashCode() ^ 0x0a45ff31;
			}
		}
		
		/// <summary> A TokenStream using the default attribute factory.</summary>
		protected internal TokenStream():base(onlyUseNewAPI?AttributeFactory.DEFAULT_ATTRIBUTE_FACTORY:TokenStream.DEFAULT_TOKEN_WRAPPER_ATTRIBUTE_FACTORY)
		{
			InitBlock();
			tokenWrapper = InitTokenWrapper(null);
			Check();
		}
		
		/// <summary> A TokenStream that uses the same attributes as the supplied one.</summary>
		protected internal TokenStream(AttributeSource input):base(input)
		{
			InitBlock();
			tokenWrapper = InitTokenWrapper(input);
			Check();
		}
		
		/// <summary> A TokenStream using the supplied AttributeFactory for creating new {@link Attribute} instances.</summary>
		protected internal TokenStream(AttributeFactory factory):base(onlyUseNewAPI?factory:new TokenWrapperAttributeFactory(factory))
		{
			InitBlock();
			tokenWrapper = InitTokenWrapper(null);
			Check();
		}
		
		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private TokenWrapper InitTokenWrapper(AttributeSource input)
		{
			if (onlyUseNewAPI)
			{
				// no wrapper needed
				return null;
			}
			else
			{
				// if possible get the wrapper from the filter's input stream
				if (input is TokenStream && ((TokenStream) input).tokenWrapper != null)
				{
					return ((TokenStream) input).tokenWrapper;
				}
				// check that all attributes are implemented by the same TokenWrapper instance
				Attribute att = AddAttribute(typeof(TermAttribute));
				if (att is TokenWrapper && AddAttribute(typeof(TypeAttribute)) == att && AddAttribute(typeof(PositionIncrementAttribute)) == att && AddAttribute(typeof(FlagsAttribute)) == att && AddAttribute(typeof(OffsetAttribute)) == att && AddAttribute(typeof(PayloadAttribute)) == att)
				{
					return (TokenWrapper) att;
				}
				else
				{
					throw new System.NotSupportedException("If onlyUseNewAPI is disabled, all basic Attributes must be implemented by the internal class " + "TokenWrapper. Please make sure, that all TokenStreams/TokenFilters in this chain have been " + "instantiated with this flag disabled and do not add any custom instances for the basic Attributes!");
				}
			}
		}
		
		/// <deprecated> Remove this when old API is removed! 
		/// </deprecated>
        [Obsolete("Remove this when old API is removed! ")]
		private void  Check()
		{
			if (onlyUseNewAPI && !supportedMethods.hasIncrementToken)
			{
				throw new System.NotSupportedException(GetType().FullName + " does not implement incrementToken() which is needed for onlyUseNewAPI.");
			}
			
			// a TokenStream subclass must at least implement one of the methods!
			if (!(supportedMethods.hasIncrementToken || supportedMethods.hasNext || supportedMethods.hasReusableNext))
			{
				throw new System.NotSupportedException(GetType().FullName + " does not implement any of incrementToken(), next(Token), next().");
			}
		}
		
		/// <summary> For extra performance you can globally enable the new
		/// {@link #IncrementToken} API using {@link Attribute}s. There will be a
		/// small, but in most cases negligible performance increase by enabling this,
		/// but it only works if <b>all</b> <code>TokenStream</code>s use the new API and
		/// implement {@link #IncrementToken}. This setting can only be enabled
		/// globally.
		/// <p/>
		/// This setting only affects <code>TokenStream</code>s instantiated after this
		/// call. All <code>TokenStream</code>s already created use the other setting.
		/// <p/>
		/// All core {@link Analyzer}s are compatible with this setting, if you have
		/// your own <code>TokenStream</code>s that are also compatible, you should enable
		/// this.
		/// <p/>
		/// When enabled, tokenization may throw {@link UnsupportedOperationException}
		/// s, if the whole tokenizer chain is not compatible eg one of the
		/// <code>TokenStream</code>s does not implement the new <code>TokenStream</code> API.
		/// <p/>
		/// The default is <code>false</code>, so there is the fallback to the old API
		/// available.
		/// 
		/// </summary>
		/// <deprecated> This setting will no longer be needed in Lucene 3.0 as the old
		/// API will be removed.
		/// </deprecated>
        [Obsolete("This setting will no longer be needed in Lucene 3.0 as the old API will be removed.")]
		public static void  SetOnlyUseNewAPI(bool onlyUseNewAPI)
		{
			TokenStream.onlyUseNewAPI = onlyUseNewAPI;
		}
		
		/// <summary> Returns if only the new API is used.
		/// 
		/// </summary>
		/// <seealso cref="setOnlyUseNewAPI">
		/// </seealso>
		/// <deprecated> This setting will no longer be needed in Lucene 3.0 as
		/// the old API will be removed.
		/// </deprecated>
        [Obsolete("This setting will no longer be needed in Lucene 3.0 as the old API will be removed.")]
		public static bool GetOnlyUseNewAPI()
		{
			return onlyUseNewAPI;
		}
		
		/// <summary> Consumers (i.e., {@link IndexWriter}) use this method to advance the stream to
		/// the next token. Implementing classes must implement this method and update
		/// the appropriate {@link AttributeImpl}s with the attributes of the next
		/// token.
		/// 
		/// The producer must make no assumptions about the attributes after the
		/// method has been returned: the caller may arbitrarily change it. If the
		/// producer needs to preserve the state for subsequent calls, it can use
		/// {@link #captureState} to create a copy of the current attribute state.
		/// 
		/// This method is called for every token of a document, so an efficient
		/// implementation is crucial for good performance. To avoid calls to
		/// {@link #AddAttribute(Class)} and {@link #GetAttribute(Class)} or downcasts,
		/// references to all {@link AttributeImpl}s that this stream uses should be
		/// retrieved during instantiation.
		/// 
		/// To ensure that filters and consumers know which attributes are available,
		/// the attributes must be added during instantiation. Filters and consumers
		/// are not required to check for availability of attributes in
		/// {@link #IncrementToken()}.
		/// 
		/// </summary>
		/// <returns> false for end of stream; true otherwise
		/// 
		/// Note that this method will be defined abstract in Lucene
		/// 3.0.
		/// </returns>
		public virtual bool IncrementToken()
		{
			System.Diagnostics.Debug.Assert(tokenWrapper != null);
			
			Token token;
			if (supportedMethods.hasReusableNext)
			{
				token = Next(tokenWrapper.delegate_Renamed);
			}
			else
			{
				System.Diagnostics.Debug.Assert(supportedMethods.hasNext);
				token = Next();
			}
			if (token == null)
				return false;
			tokenWrapper.delegate_Renamed = token;
			return true;
		}
		
		/// <summary> This method is called by the consumer after the last token has been
		/// consumed, after {@link #IncrementToken()} returned <code>false</code>
		/// (using the new <code>TokenStream</code> API). Streams implementing the old API
		/// should upgrade to use this feature.
		/// <p/>
		/// This method can be used to perform any end-of-stream operations, such as
		/// setting the final offset of a stream. The final offset of a stream might
		/// differ from the offset of the last token eg in case one or more whitespaces
		/// followed after the last token, but a {@link WhitespaceTokenizer} was used.
		/// 
		/// </summary>
		/// <throws>  IOException </throws>
		public virtual void  End()
		{
			// do nothing by default
		}
		
		/// <summary> Returns the next token in the stream, or null at EOS. When possible, the
		/// input Token should be used as the returned Token (this gives fastest
		/// tokenization performance), but this is not required and a new Token may be
		/// returned. Callers may re-use a single Token instance for successive calls
		/// to this method.
		/// 
		/// This implicitly defines a "contract" between consumers (callers of this
		/// method) and producers (implementations of this method that are the source
		/// for tokens):
		/// <ul>
		/// <li>A consumer must fully consume the previously returned {@link Token}
		/// before calling this method again.</li>
		/// <li>A producer must call {@link Token#Clear()} before setting the fields in
		/// it and returning it</li>
		/// </ul>
		/// Also, the producer must make no assumptions about a {@link Token} after it
		/// has been returned: the caller may arbitrarily change it. If the producer
		/// needs to hold onto the {@link Token} for subsequent calls, it must clone()
		/// it before storing it. Note that a {@link TokenFilter} is considered a
		/// consumer.
		/// 
		/// </summary>
		/// <param name="reusableToken">a {@link Token} that may or may not be used to return;
		/// this parameter should never be null (the callee is not required to
		/// check for null before using it, but it is a good idea to assert that
		/// it is not null.)
		/// </param>
		/// <returns> next {@link Token} in the stream or null if end-of-stream was hit
		/// </returns>
		/// <deprecated> The new {@link #IncrementToken()} and {@link AttributeSource}
		/// APIs should be used instead.
		/// </deprecated>
        [Obsolete("The new IncrementToken() and AttributeSource APIs should be used instead.")]
		public virtual Token Next(Token reusableToken)
		{
			System.Diagnostics.Debug.Assert(reusableToken != null);
			
			if (tokenWrapper == null)
				throw new System.NotSupportedException("This TokenStream only supports the new Attributes API.");
			
			if (supportedMethods.hasIncrementToken)
			{
				tokenWrapper.delegate_Renamed = reusableToken;
				return IncrementToken()?tokenWrapper.delegate_Renamed:null;
			}
			else
			{
				System.Diagnostics.Debug.Assert(supportedMethods.hasNext);
				return Next();
			}
		}
		
		/// <summary> Returns the next {@link Token} in the stream, or null at EOS.
		/// 
		/// </summary>
		/// <deprecated> The returned Token is a "full private copy" (not re-used across
		/// calls to {@link #Next()}) but will be slower than calling
		/// {@link #Next(Token)} or using the new {@link #IncrementToken()}
		/// method with the new {@link AttributeSource} API.
		/// </deprecated>
        [Obsolete("The returned Token is a \"full private copy\" (not re-used across calls to Next()) but will be slower than calling {@link #Next(Token)} or using the new IncrementToken() method with the new AttributeSource API.")]
		public virtual Token Next()
		{
			if (tokenWrapper == null)
				throw new System.NotSupportedException("This TokenStream only supports the new Attributes API.");
			
			Token nextToken;
			if (supportedMethods.hasIncrementToken)
			{
				Token savedDelegate = tokenWrapper.delegate_Renamed;
				tokenWrapper.delegate_Renamed = new Token();
				nextToken = IncrementToken()?tokenWrapper.delegate_Renamed:null;
				tokenWrapper.delegate_Renamed = savedDelegate;
			}
			else
			{
				System.Diagnostics.Debug.Assert(supportedMethods.hasReusableNext);
				nextToken = Next(new Token());
			}
			
			if (nextToken != null)
			{
				Mono.Lucene.Net.Index.Payload p = nextToken.GetPayload();
				if (p != null)
				{
					nextToken.SetPayload((Mono.Lucene.Net.Index.Payload) p.Clone());
				}
			}
			return nextToken;
		}
		
		/// <summary> Resets this stream to the beginning. This is an optional operation, so
		/// subclasses may or may not implement this method. {@link #Reset()} is not needed for
		/// the standard indexing process. However, if the tokens of a
		/// <code>TokenStream</code> are intended to be consumed more than once, it is
		/// necessary to implement {@link #Reset()}. Note that if your TokenStream
		/// caches tokens and feeds them back again after a reset, it is imperative
		/// that you clone the tokens when you store them away (on the first pass) as
		/// well as when you return them (on future passes after {@link #Reset()}).
		/// </summary>
		public virtual void  Reset()
		{
		}
		
		/// <summary>Releases resources associated with this stream. </summary>
		public virtual void  Close()
		{
		}
	}
}
