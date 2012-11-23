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

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> This exception is thrown when an {@link IndexReader}
	/// tries to make changes to the index (via {@link
	/// IndexReader#deleteDocument}, {@link
	/// IndexReader#undeleteAll} or {@link IndexReader#setNorm})
	/// but changes have already been committed to the index
	/// since this reader was instantiated.  When this happens
	/// you must open a new reader on the current index to make
	/// the changes.
	/// </summary>
	[Serializable]
	public class StaleReaderException:System.IO.IOException
	{
		public StaleReaderException(System.String message):base(message)
		{
		}
	}
}
