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
	
	/// <summary>Extends <code>TermFreqVector</code> to provide additional information about
	/// positions in which each of the terms is found.
	/// </summary>
	public interface TermPositionVector:TermFreqVector
	{
		
		/// <summary>Returns an array of positions in which the term is found.
		/// Terms are identified by the index at which its number appears in the
		/// term number array obtained from <code>getTermNumbers</code> method.
		/// </summary>
		int[] GetTermPositions(int index);
	}
}