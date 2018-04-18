/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.,  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

//
// System.DirectoryServices.ResultPropertyCollection .cs
//
// Author:
//   Sunil Kumar (sunilk@novell.com)
//
// (C)  Novell Inc.
//

using System;
using System.Collections;

namespace System.DirectoryServices
{
	/// <summary>
	/// Contains the properties of a SearchResult instance.	
	/// 
	/// For a list of all members of this type, see ResultPropertyCollection 
	/// Members.
	/// </summary>
	/// <remarks>
	/// SearchResult instances are similar to DirectoryEntry instances. The 
	/// notable difference is that the DirectoryEntry retrieves its 
	/// information from the Active Directory hierarchy each time a new object 
	/// is accessed, whereas the data for the SearchResult is already 
	/// available in the SearchResultCollection that a DirectorySearcher 
	/// query returns. If you try to get a SearchResult property that your 
	/// query did not specify for retrieval, the property will not be 
	/// available.
	/// </remarks>
	public class ResultPropertyCollection  : DictionaryBase
	{
		internal ResultPropertyCollection()
		{
			
		}

		public ResultPropertyValueCollection this[string name]
		{
			get {
				return (ResultPropertyValueCollection) this.Dictionary[name.ToLower()];
			}
//			set { this.Dictionary[key] = value; } 
		}
		//add a ResultPropertyValueCollection based on key 
		internal void Add(string key, ResultPropertyValueCollection rpcoll) 
		{ 
			this.Dictionary.Add(key.ToLower(), rpcoll); 
		} 
		
		//see if collection contains an entry corresponding to key
		public bool Contains(string propertyName)
		{
			return this.Dictionary.Contains(propertyName.ToLower());
		}
		
		public ICollection PropertyNames 
		{
			get
			{
				return this.Dictionary.Keys;
			}
		}

		public ICollection Values 
		{
			get
			{
				return this.Dictionary.Values;
			}
		}

		public void CopyTo (ResultPropertyValueCollection[] array, int index)
		{
			foreach (ResultPropertyValueCollection vals in Values)
				array[index++] = vals;
		}
	}
}
