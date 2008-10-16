/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
namespace Monodoc.Lucene.Net.Index
{
	sealed public class FieldInfo
	{
		internal System.String name;
		internal bool isIndexed;
		internal int number;
		
		// true if term vector for this Field should be stored
		public /*internal*/ bool storeTermVector;
		
		internal FieldInfo(System.String na, bool tk, int nu, bool storeTermVector)
		{
			name = na;
			isIndexed = tk;
			number = nu;
			this.storeTermVector = storeTermVector;
		}
	}
}