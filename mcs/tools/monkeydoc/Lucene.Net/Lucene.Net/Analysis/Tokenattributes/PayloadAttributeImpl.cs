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

using Payload = Mono.Lucene.Net.Index.Payload;
using AttributeImpl = Mono.Lucene.Net.Util.AttributeImpl;

namespace Mono.Lucene.Net.Analysis.Tokenattributes
{
	
	/// <summary> The payload of a Token. See also {@link Payload}.</summary>
	[Serializable]
	public class PayloadAttributeImpl:AttributeImpl, PayloadAttribute, System.ICloneable
	{
		private Payload payload;
		
		/// <summary> Initialize this attribute with no payload.</summary>
		public PayloadAttributeImpl()
		{
		}
		
		/// <summary> Initialize this attribute with the given payload. </summary>
		public PayloadAttributeImpl(Payload payload)
		{
			this.payload = payload;
		}
		
		/// <summary> Returns this Token's payload.</summary>
		public virtual Payload GetPayload()
		{
			return this.payload;
		}
		
		/// <summary> Sets this Token's payload.</summary>
		public virtual void  SetPayload(Payload payload)
		{
			this.payload = payload;
		}
		
		public override void  Clear()
		{
			payload = null;
		}
		
		public override System.Object Clone()
		{
            PayloadAttributeImpl impl = new PayloadAttributeImpl();
            impl.payload = new Payload(this.payload.data, this.payload.offset, this.payload.length);
            return impl;
		}
		
		public  override bool Equals(System.Object other)
		{
			if (other == this)
			{
				return true;
			}
			
			if (other is PayloadAttribute)
			{
				PayloadAttributeImpl o = (PayloadAttributeImpl) other;
				if (o.payload == null || payload == null)
				{
					return o.payload == null && payload == null;
				}
				
				return o.payload.Equals(payload);
			}
			
			return false;
		}
		
		public override int GetHashCode()
		{
			return (payload == null)?0:payload.GetHashCode();
		}
		
		public override void  CopyTo(AttributeImpl target)
		{
			PayloadAttribute t = (PayloadAttribute) target;
			t.SetPayload((payload == null)?null:(Payload) payload.Clone());
		}
	}
}
