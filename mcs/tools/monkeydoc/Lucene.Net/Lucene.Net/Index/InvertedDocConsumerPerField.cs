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

namespace Mono.Lucene.Net.Index
{
	
	abstract class InvertedDocConsumerPerField
	{
		
		// Called once per field, and is given all Fieldable
		// occurrences for this field in the document.  Return
		// true if you wish to see inverted tokens for these
		// fields:
		internal abstract bool Start(Fieldable[] fields, int count);
		
		// Called before a field instance is being processed
		internal abstract void  Start(Fieldable field);
		
		// Called once per inverted token
		internal abstract void  Add();
		
		// Called once per field per document, after all Fieldable
		// occurrences are inverted
		internal abstract void  Finish();
		
		// Called on hitting an aborting exception
		public abstract void  Abort();
	}
}
