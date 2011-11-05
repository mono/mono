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

using IndexInput = Mono.Lucene.Net.Store.IndexInput;

namespace Mono.Lucene.Net.Index
{
	
	abstract class FormatPostingsPositionsConsumer
	{
		
		/// <summary>Add a new position &amp; payload.  If payloadLength > 0
		/// you must read those bytes from the IndexInput. 
		/// </summary>
		internal abstract void  AddPosition(int position, byte[] payload, int payloadOffset, int payloadLength);
		
		/// <summary>Called when we are done adding positions &amp; payloads </summary>
		internal abstract void  Finish();
	}
}
