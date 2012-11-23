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

using OffsetAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.OffsetAttribute;
using TermAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TermAttribute;
using AttributeSource = Mono.Lucene.Net.Util.AttributeSource;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> Emits the entire input as a single token.</summary>
	public class KeywordTokenizer:Tokenizer
	{
		
		private const int DEFAULT_BUFFER_SIZE = 256;
		
		private bool done;
		private int finalOffset;
		private TermAttribute termAtt;
		private OffsetAttribute offsetAtt;
		
		public KeywordTokenizer(System.IO.TextReader input):this(input, DEFAULT_BUFFER_SIZE)
		{
		}
		
		public KeywordTokenizer(System.IO.TextReader input, int bufferSize):base(input)
		{
			Init(bufferSize);
		}
		
		public KeywordTokenizer(AttributeSource source, System.IO.TextReader input, int bufferSize):base(source, input)
		{
			Init(bufferSize);
		}
		
		public KeywordTokenizer(AttributeFactory factory, System.IO.TextReader input, int bufferSize):base(factory, input)
		{
			Init(bufferSize);
		}
		
		private void  Init(int bufferSize)
		{
			this.done = false;
			termAtt = (TermAttribute) AddAttribute(typeof(TermAttribute));
			offsetAtt = (OffsetAttribute) AddAttribute(typeof(OffsetAttribute));
			termAtt.ResizeTermBuffer(bufferSize);
		}
		
		public override bool IncrementToken()
		{
			if (!done)
			{
				ClearAttributes();
				done = true;
				int upto = 0;
				char[] buffer = termAtt.TermBuffer();
				while (true)
				{
					int length = input.Read(buffer, upto, buffer.Length - upto);
					if (length == 0)
						break;
					upto += length;
					if (upto == buffer.Length)
						buffer = termAtt.ResizeTermBuffer(1 + buffer.Length);
				}
				termAtt.SetTermLength(upto);
				finalOffset = CorrectOffset(upto);
				offsetAtt.SetOffset(CorrectOffset(0), finalOffset);
				return true;
			}
			return false;
		}
		
		public override void  End()
		{
			// set final offset 
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
		
		public override void  Reset(System.IO.TextReader input)
		{
			base.Reset(input);
			this.done = false;
		}
	}
}
