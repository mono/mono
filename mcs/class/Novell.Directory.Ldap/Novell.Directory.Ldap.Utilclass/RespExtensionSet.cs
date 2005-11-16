/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
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
// Novell.Directory.Ldap.Utilclass.RespExtensionSet.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
#if TARGET_JVM
    // This dummy class workarounds a MS CSC bug by using SupportClass before
	// using its inner class (SupportClass.AbstractSetSupport)
	class RespExtensionSetDummy : SupportClass {}
#endif

	/// <summary> This  class  extends the AbstractSet and Implements the Set
	/// so that it can be used to maintain a list of currently
	/// registered extended responses.
	/// </summary>
	public class RespExtensionSet:SupportClass.AbstractSetSupport
	{
		/// <summary> Returns the number of extensions in this set.
		/// 
		/// </summary>
		/// <returns> number of extensions in this set.
		/// </returns>
		public override int Count
		{
			get
			{
				return this.map.Count;
			}
			
		}
		
		private System.Collections.Hashtable map;
		
		public RespExtensionSet():base()
		{
			map = new System.Collections.Hashtable();
			return ;
		}
		
		
		/* Adds a responseExtension to the current list of registered responses.
		*
		*/
		public void  registerResponseExtension(System.String oid, System.Type extClass)
		{
			lock (this)
			{
				
				if (!this.map.ContainsKey(oid))
				{
					this.map.Add(oid, (System.Type) extClass);
				}
			}
		}
		
		/// <summary> Returns an iterator over the responses in this set.  The responses
		/// returned from this iterator are not in any particular order.
		/// 
		/// </summary>
		/// <returns> iterator over the responses in this set
		/// </returns>
		public override System.Collections.IEnumerator GetEnumerator()
		{
			return this.map.Values.GetEnumerator();
		}
		
		/* Searches the list of registered responses for a mathcing response.  We
		* search using the OID string.  If a match is found we return the
		* Class name that was provided to us on registration.
		*/
		public System.Type findResponseExtension(System.String searchOID)
		{
			lock (this)
			{
				if (this.map.ContainsKey(searchOID))
				{
					return (System.Type) this.map[searchOID];
				}
				/* The requested extension does not have a registered response class */
				return null;
			}
		}
	}
}
