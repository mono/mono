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
// System.DirectoryServices.SearchResult.cs
//
// Author:
//   Sunil Kumar (sunilk@novell.com)
//
// (C)  Novell Inc.
//

using System.ComponentModel;

namespace System.DirectoryServices
{
	
	/// <summary>
	///Encapsulates a node or object in the Ldap Directory hierarchy.
	/// </summary>
    public class SearchResult
	{

		private string _Path=null;
		private Properties _Properties=null;

		/// <summary>
		/// Gets the path for this SearchResult.
		/// </summary>
		/// <value>
		/// The path of this SearchResult.
		/// </value>
		/// <remarks>
		/// The Path property uniquely identifies this entry in the Active 
		/// Directory hierarchy. The entry can always be retrieved using this 
		/// path
		/// </remarks>
		public string Path 
		{
			get
			{
				return _Path;
			}
		}

		/// <summary>
		/// Gets a ResultPropertyCollection of properties set on this object.
		/// </summary>
		/// <value>
		/// A ResultPropertyCollection of properties set on this object.
		/// </value>
		/// <remarks>
		/// This collection only contains properties that were explicitly 
		/// requested through DirectorySearcher.PropertiesToLoad.
		/// </remarks>
		public ResultPropertyCollection Properties 
		{
			get
			{
				return _Properties;
			}
		}

		/// <summary>
		/// Retrieves the DirectoryEntry that corresponds to the SearchResult, 
		/// from the Active Directory hierarchy.
		/// </summary>
		/// <returns>
		/// The DirectoryEntry that corresponds to the SearchResult
		/// </returns>
		/// <remarks>
		/// Use GetDirectoryEntry when you want to look at the live entry 
		/// instead of the entry returned through DirectorySearcher, or when 
		/// you want to invoke a method on the object that was returned.
		/// </remarks>
		public DirectoryEntry GetDirectoryEntry()
		{
		}

	}
}

