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

using AttributeImpl = Mono.Lucene.Net.Util.AttributeImpl;

namespace Mono.Lucene.Net.Analysis.Tokenattributes
{
	
	/// <summary> A Token's lexical type. The Default value is "word". </summary>
	[Serializable]
	public class TypeAttributeImpl:AttributeImpl, TypeAttribute, System.ICloneable
	{
		private System.String type;
		public const System.String DEFAULT_TYPE = "word";
		
		public TypeAttributeImpl():this(DEFAULT_TYPE)
		{
		}
		
		public TypeAttributeImpl(System.String type)
		{
			this.type = type;
		}
		
		/// <summary>Returns this Token's lexical type.  Defaults to "word". </summary>
		public virtual System.String Type()
		{
			return type;
		}
		
		/// <summary>Set the lexical type.</summary>
		/// <seealso cref="Type()">
		/// </seealso>
		public virtual void  SetType(System.String type)
		{
			this.type = type;
		}
		
		public override void  Clear()
		{
			type = DEFAULT_TYPE;
		}
		
		public  override bool Equals(System.Object other)
		{
			if (other == this)
			{
				return true;
			}
			
			if (other is TypeAttributeImpl)
			{
				return type.Equals(((TypeAttributeImpl) other).type);
			}
			
			return false;
		}
		
		public override int GetHashCode()
		{
			return type.GetHashCode();
		}
		
		public override void  CopyTo(AttributeImpl target)
		{
			TypeAttribute t = (TypeAttribute) target;
			t.SetType(type);
		}
		
		override public System.Object Clone()
		{
            TypeAttributeImpl impl = new TypeAttributeImpl();
            impl.type = type;
            return impl;
		}
	}
}
