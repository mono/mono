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

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> Subclasses of StringInterner are required to
	/// return the same single String object for all equal strings.
	/// Depending on the implementation, this may not be
	/// the same object returned as String.intern().
	/// 
	/// This StringInterner base class simply delegates to String.intern().
	/// </summary>
	public class StringInterner
	{
		/// <summary>Returns a single object instance for each equal string. </summary>
		public virtual System.String Intern(System.String s)
		{
			return String.Intern(s);
		}
		
		/// <summary>Returns a single object instance for each equal string. </summary>
		public virtual System.String Intern(char[] arr, int offset, int len)
		{
			return Intern(new System.String(arr, offset, len));
		}
	}
}
