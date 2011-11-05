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
using Payload = Mono.Lucene.Net.Index.Payload;
using AttributeImpl = Mono.Lucene.Net.Util.AttributeImpl;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> This class wraps a Token and supplies a single attribute instance
	/// where the delegate token can be replaced.
	/// </summary>
	/// <deprecated> Will be removed, when old TokenStream API is removed.
	/// </deprecated>
    [Obsolete("Will be removed, when old TokenStream API is removed.")]
	[Serializable]
	public sealed class TokenWrapper:AttributeImpl, System.ICloneable, TermAttribute, TypeAttribute, PositionIncrementAttribute, FlagsAttribute, OffsetAttribute, PayloadAttribute
	{
		
		internal Token delegate_Renamed;
		
		internal TokenWrapper():this(new Token())
		{
		}
		
		internal TokenWrapper(Token delegate_Renamed)
		{
			this.delegate_Renamed = delegate_Renamed;
		}
		
		// TermAttribute:
		
		public System.String Term()
		{
			return delegate_Renamed.Term();
		}
		
		public void  SetTermBuffer(char[] buffer, int offset, int length)
		{
			delegate_Renamed.SetTermBuffer(buffer, offset, length);
		}
		
		public void  SetTermBuffer(System.String buffer)
		{
			delegate_Renamed.SetTermBuffer(buffer);
		}
		
		public void  SetTermBuffer(System.String buffer, int offset, int length)
		{
			delegate_Renamed.SetTermBuffer(buffer, offset, length);
		}
		
		public char[] TermBuffer()
		{
			return delegate_Renamed.TermBuffer();
		}
		
		public char[] ResizeTermBuffer(int newSize)
		{
			return delegate_Renamed.ResizeTermBuffer(newSize);
		}
		
		public int TermLength()
		{
			return delegate_Renamed.TermLength();
		}
		
		public void  SetTermLength(int length)
		{
			delegate_Renamed.SetTermLength(length);
		}
		
		// TypeAttribute:
		
		public System.String Type()
		{
			return delegate_Renamed.Type();
		}
		
		public void  SetType(System.String type)
		{
			delegate_Renamed.SetType(type);
		}
		
		public void  SetPositionIncrement(int positionIncrement)
		{
			delegate_Renamed.SetPositionIncrement(positionIncrement);
		}
		
		public int GetPositionIncrement()
		{
			return delegate_Renamed.GetPositionIncrement();
		}
		
		// FlagsAttribute
		
		public int GetFlags()
		{
			return delegate_Renamed.GetFlags();
		}
		
		public void  SetFlags(int flags)
		{
			delegate_Renamed.SetFlags(flags);
		}
		
		// OffsetAttribute
		
		public int StartOffset()
		{
			return delegate_Renamed.StartOffset();
		}
		
		public void  SetOffset(int startOffset, int endOffset)
		{
			delegate_Renamed.SetOffset(startOffset, endOffset);
		}
		
		public int EndOffset()
		{
			return delegate_Renamed.EndOffset();
		}
		
		// PayloadAttribute
		
		public Payload GetPayload()
		{
			return delegate_Renamed.GetPayload();
		}
		
		public void  SetPayload(Payload payload)
		{
			delegate_Renamed.SetPayload(payload);
		}
		
		// AttributeImpl
		
		public override void  Clear()
		{
			delegate_Renamed.Clear();
		}
		
		public override System.String ToString()
		{
			return delegate_Renamed.ToString();
		}
		
		public override int GetHashCode()
		{
			return delegate_Renamed.GetHashCode();
		}
		
		public  override bool Equals(System.Object other)
		{
			if (other is TokenWrapper)
			{
				return ((TokenWrapper) other).delegate_Renamed.Equals(this.delegate_Renamed);
			}
			return false;
		}
		
		public override System.Object Clone()
		{
			return new TokenWrapper((Token) delegate_Renamed.Clone());
		}
		
		public override void  CopyTo(AttributeImpl target)
		{
			if (target is TokenWrapper)
			{
				((TokenWrapper) target).delegate_Renamed = (Token) this.delegate_Renamed.Clone();
			}
			else
			{
				this.delegate_Renamed.CopyTo(target);
			}
		}
	}
}
