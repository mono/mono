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

using CharReader = Mono.Lucene.Net.Analysis.CharReader;
using Token = Mono.Lucene.Net.Analysis.Token;
using Tokenizer = Mono.Lucene.Net.Analysis.Tokenizer;
using OffsetAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.OffsetAttribute;
using PositionIncrementAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.PositionIncrementAttribute;
using TermAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TermAttribute;
using TypeAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TypeAttribute;
using AttributeSource = Mono.Lucene.Net.Util.AttributeSource;
using Version = Mono.Lucene.Net.Util.Version;

namespace Mono.Lucene.Net.Analysis.Standard
{
	
	/// <summary>A grammar-based tokenizer constructed with JFlex
	/// 
	/// <p/> This should be a good tokenizer for most European-language documents:
	/// 
	/// <ul>
	/// <li>Splits words at punctuation characters, removing punctuation. However, a 
	/// dot that's not followed by whitespace is considered part of a token.</li>
	/// <li>Splits words at hyphens, unless there's a number in the token, in which case
	/// the whole token is interpreted as a product number and is not split.</li>
	/// <li>Recognizes email addresses and internet hostnames as one token.</li>
	/// </ul>
	/// 
	/// <p/>Many applications have specific tokenizer needs.  If this tokenizer does
	/// not suit your application, please consider copying this source code
	/// directory to your project and maintaining your own grammar-based tokenizer.
	/// 
	/// <a name="version"/>
	/// <p/>
	/// You must specify the required {@link Version} compatibility when creating
	/// StandardAnalyzer:
	/// <ul>
	/// <li>As of 2.4, Tokens incorrectly identified as acronyms are corrected (see
	/// <a href="https://issues.apache.org/jira/browse/LUCENE-1068">LUCENE-1608</a></li>
	/// </ul>
	/// </summary>
	
	public class StandardTokenizer:Tokenizer
	{
		private void  InitBlock()
		{
			maxTokenLength = StandardAnalyzer.DEFAULT_MAX_TOKEN_LENGTH;
		}
		/// <summary>A private instance of the JFlex-constructed scanner </summary>
		private StandardTokenizerImpl scanner;
		
		public const int ALPHANUM = 0;
		public const int APOSTROPHE = 1;
		public const int ACRONYM = 2;
		public const int COMPANY = 3;
		public const int EMAIL = 4;
		public const int HOST = 5;
		public const int NUM = 6;
		public const int CJ = 7;
		
		/// <deprecated> this solves a bug where HOSTs that end with '.' are identified
		/// as ACRONYMs. It is deprecated and will be removed in the next
		/// release.
		/// </deprecated>
        [Obsolete("this solves a bug where HOSTs that end with '.' are identified as ACRONYMs. It is deprecated and will be removed in the next release.")]
		public const int ACRONYM_DEP = 8;
		
		/// <summary>String token types that correspond to token type int constants </summary>
		public static readonly System.String[] TOKEN_TYPES = new System.String[]{"<ALPHANUM>", "<APOSTROPHE>", "<ACRONYM>", "<COMPANY>", "<EMAIL>", "<HOST>", "<NUM>", "<CJ>", "<ACRONYM_DEP>"};
		
		/// <deprecated> Please use {@link #TOKEN_TYPES} instead 
		/// </deprecated>
        [Obsolete("Please use TOKEN_TYPES instead")]
		public static readonly System.String[] tokenImage = TOKEN_TYPES;
		
		/// <summary> Specifies whether deprecated acronyms should be replaced with HOST type.
		/// This is false by default to support backward compatibility.
		/// <p/>
		/// See http://issues.apache.org/jira/browse/LUCENE-1068
		/// 
		/// </summary>
		/// <deprecated> this should be removed in the next release (3.0).
		/// </deprecated>
        [Obsolete("this should be removed in the next release (3.0).")]
		private bool replaceInvalidAcronym;
		
		private int maxTokenLength;
		
		/// <summary>Set the max allowed token length.  Any token longer
		/// than this is skipped. 
		/// </summary>
		public virtual void  SetMaxTokenLength(int length)
		{
			this.maxTokenLength = length;
		}
		
		/// <seealso cref="setMaxTokenLength">
		/// </seealso>
		public virtual int GetMaxTokenLength()
		{
			return maxTokenLength;
		}
		
		/// <summary> Creates a new instance of the {@link StandardTokenizer}. Attaches the
		/// <code>input</code> to a newly created JFlex scanner.
		/// </summary>
		/// <deprecated> Use {@link #StandardTokenizer(Version, Reader)} instead
		/// </deprecated>
        [Obsolete("Use StandardTokenizer(Version, Reader) instead")]
		public StandardTokenizer(System.IO.TextReader input):this(Version.LUCENE_24, input)
		{
		}
		
		/// <summary> Creates a new instance of the {@link Mono.Lucene.Net.Analysis.Standard.StandardTokenizer}.  Attaches
		/// the <code>input</code> to the newly created JFlex scanner.
		/// 
		/// </summary>
		/// <param name="input">The input reader
		/// </param>
		/// <param name="replaceInvalidAcronym">Set to true to replace mischaracterized acronyms with HOST.
		/// 
		/// See http://issues.apache.org/jira/browse/LUCENE-1068
		/// </param>
		/// <deprecated> Use {@link #StandardTokenizer(Version, Reader)} instead
		/// </deprecated>
        [Obsolete("Use StandardTokenizer(Version, Reader) instead")]
		public StandardTokenizer(System.IO.TextReader input, bool replaceInvalidAcronym):base()
		{
			InitBlock();
			this.scanner = new StandardTokenizerImpl(input);
			Init(input, replaceInvalidAcronym);
		}
		
		/// <summary> Creates a new instance of the
		/// {@link org.apache.lucene.analysis.standard.StandardTokenizer}. Attaches
		/// the <code>input</code> to the newly created JFlex scanner.
		/// 
		/// </summary>
		/// <param name="input">The input reader
		/// 
		/// See http://issues.apache.org/jira/browse/LUCENE-1068
		/// </param>
		public StandardTokenizer(Version matchVersion, System.IO.TextReader input):base()
		{
			InitBlock();
			this.scanner = new StandardTokenizerImpl(input);
			Init(input, matchVersion);
		}
		
		/// <summary> Creates a new StandardTokenizer with a given {@link AttributeSource}. </summary>
		/// <deprecated> Use
		/// {@link #StandardTokenizer(Version, AttributeSource, Reader)}
		/// instead
		/// </deprecated>
        [Obsolete("Use StandardTokenizer(Version, AttributeSource, Reader) instead")]
		public StandardTokenizer(AttributeSource source, System.IO.TextReader input, bool replaceInvalidAcronym):base(source)
		{
			InitBlock();
			this.scanner = new StandardTokenizerImpl(input);
			Init(input, replaceInvalidAcronym);
		}
		
		/// <summary> Creates a new StandardTokenizer with a given {@link AttributeSource}.</summary>
		public StandardTokenizer(Version matchVersion, AttributeSource source, System.IO.TextReader input):base(source)
		{
			InitBlock();
			this.scanner = new StandardTokenizerImpl(input);
			Init(input, matchVersion);
		}
		
		/// <summary> Creates a new StandardTokenizer with a given {@link Mono.Lucene.Net.Util.AttributeSource.AttributeFactory} </summary>
		/// <deprecated> Use
		/// {@link #StandardTokenizer(Version, org.apache.lucene.util.AttributeSource.AttributeFactory, Reader)}
		/// instead
		/// </deprecated>
        [Obsolete("Use StandardTokenizer(Version, Mono.Lucene.Net.Util.AttributeSource.AttributeFactory, Reader) instead")]
		public StandardTokenizer(AttributeFactory factory, System.IO.TextReader input, bool replaceInvalidAcronym):base(factory)
		{
			InitBlock();
			this.scanner = new StandardTokenizerImpl(input);
			Init(input, replaceInvalidAcronym);
		}
		
		/// <summary> Creates a new StandardTokenizer with a given
		/// {@link org.apache.lucene.util.AttributeSource.AttributeFactory}
		/// </summary>
		public StandardTokenizer(Version matchVersion, AttributeFactory factory, System.IO.TextReader input):base(factory)
		{
			InitBlock();
			this.scanner = new StandardTokenizerImpl(input);
			Init(input, matchVersion);
		}
		
		private void  Init(System.IO.TextReader input, bool replaceInvalidAcronym)
		{
			this.replaceInvalidAcronym = replaceInvalidAcronym;
			this.input = input;
			termAtt = (TermAttribute) AddAttribute(typeof(TermAttribute));
			offsetAtt = (OffsetAttribute) AddAttribute(typeof(OffsetAttribute));
			posIncrAtt = (PositionIncrementAttribute) AddAttribute(typeof(PositionIncrementAttribute));
			typeAtt = (TypeAttribute) AddAttribute(typeof(TypeAttribute));
		}
		
		private void  Init(System.IO.TextReader input, Version matchVersion)
		{
			if (matchVersion.OnOrAfter(Version.LUCENE_24))
			{
				Init(input, true);
			}
			else
			{
				Init(input, false);
			}
		}
		
		// this tokenizer generates three attributes:
		// offset, positionIncrement and type
		private TermAttribute termAtt;
		private OffsetAttribute offsetAtt;
		private PositionIncrementAttribute posIncrAtt;
		private TypeAttribute typeAtt;
		
		/*
		* (non-Javadoc)
		*
		* @see Mono.Lucene.Net.Analysis.TokenStream#next()
		*/
		public override bool IncrementToken()
		{
			ClearAttributes();
			int posIncr = 1;
			
			while (true)
			{
				int tokenType = scanner.GetNextToken();
				
				if (tokenType == StandardTokenizerImpl.YYEOF)
				{
					return false;
				}
				
				if (scanner.Yylength() <= maxTokenLength)
				{
					posIncrAtt.SetPositionIncrement(posIncr);
					scanner.GetText(termAtt);
					int start = scanner.Yychar();
					offsetAtt.SetOffset(CorrectOffset(start), CorrectOffset(start + termAtt.TermLength()));
					// This 'if' should be removed in the next release. For now, it converts
					// invalid acronyms to HOST. When removed, only the 'else' part should
					// remain.
					if (tokenType == StandardTokenizerImpl.ACRONYM_DEP)
					{
						if (replaceInvalidAcronym)
						{
							typeAtt.SetType(StandardTokenizerImpl.TOKEN_TYPES[StandardTokenizerImpl.HOST]);
							termAtt.SetTermLength(termAtt.TermLength() - 1); // remove extra '.'
						}
						else
						{
							typeAtt.SetType(StandardTokenizerImpl.TOKEN_TYPES[StandardTokenizerImpl.ACRONYM]);
						}
					}
					else
					{
						typeAtt.SetType(StandardTokenizerImpl.TOKEN_TYPES[tokenType]);
					}
					return true;
				}
				// When we skip a too-long term, we still increment the
				// position increment
				else
					posIncr++;
			}
		}
		
		public override void  End()
		{
			// set final offset
			int finalOffset = CorrectOffset(scanner.Yychar() + scanner.Yylength());
			offsetAtt.SetOffset(finalOffset, finalOffset);
		}
		
		/// <deprecated> Will be removed in Lucene 3.0. This method is final, as it should
		/// not be overridden. Delegates to the backwards compatibility layer. 
		/// </deprecated>
        [Obsolete("Will be removed in Lucene 3.0. This method is final, as it should not be overridden. Delegates to the backwards compatibility layer. ")]
		public override Token Next(Token reusableToken)
		{
			return base.Next(reusableToken);
		}
		
		/// <deprecated> Will be removed in Lucene 3.0. This method is final, as it should
		/// not be overridden. Delegates to the backwards compatibility layer. 
		/// </deprecated>
        [Obsolete("Will be removed in Lucene 3.0. This method is final, as it should not be overridden. Delegates to the backwards compatibility layer. ")]
		public override Token Next()
		{
			return base.Next();
		}
		
				
		public override void  Reset(System.IO.TextReader reader)
		{
			base.Reset(reader);
			scanner.Reset(reader);
		}
		
		/// <summary> Prior to https://issues.apache.org/jira/browse/LUCENE-1068, StandardTokenizer mischaracterized as acronyms tokens like www.abc.com
		/// when they should have been labeled as hosts instead.
		/// </summary>
		/// <returns> true if StandardTokenizer now returns these tokens as Hosts, otherwise false
		/// 
		/// </returns>
		/// <deprecated> Remove in 3.X and make true the only valid value
		/// </deprecated>
        [Obsolete("Remove in 3.X and make true the only valid value")]
		public virtual bool IsReplaceInvalidAcronym()
		{
			return replaceInvalidAcronym;
		}
		
		/// <summary> </summary>
		/// <param name="replaceInvalidAcronym">Set to true to replace mischaracterized acronyms as HOST.
		/// </param>
		/// <deprecated> Remove in 3.X and make true the only valid value
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// </deprecated>
        [Obsolete("Remove in 3.X and make true the only valid value. See https://issues.apache.org/jira/browse/LUCENE-1068")]
		public virtual void  SetReplaceInvalidAcronym(bool replaceInvalidAcronym)
		{
			this.replaceInvalidAcronym = replaceInvalidAcronym;
		}
	}
}
