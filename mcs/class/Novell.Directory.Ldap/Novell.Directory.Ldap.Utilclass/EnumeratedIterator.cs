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
// Novell.Directory.Ldap.Utilclass.EnumeratedIterator.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	/// <summary> wrappers a class of type Iterator and makes it act as an Enumerator.  This
	/// is used when the API requires enumerations be used but we may be using
	/// JDK1.2 collections, which return iterators instead of enumerators.  Used by
	/// LdapSchema and LdapSchemaElement
	/// 
	/// </summary>
	/// <seealso cref="Novell.Directory.Ldap.LdapSchema.AttributeSchemas">
	/// </seealso>
	/// <seealso cref="Novell.Directory.Ldap.LdapSchemaElement.QualifierNames">
	/// </seealso>
	
	public class EnumeratedIterator : System.Collections.IEnumerator
	{
		private System.Object tempAuxObj;
		public virtual bool MoveNext()
		{
			bool result = hasMoreElements();
			if (result)
			{
				tempAuxObj = nextElement();
			}
			return result;
		}
		public virtual void  Reset()
		{
			tempAuxObj = null;
		}
		public virtual System.Object Current
		{
			get
			{
				return tempAuxObj;
			}
			
		}
		private System.Collections.IEnumerator i;
		
		public EnumeratedIterator(System.Collections.IEnumerator iterator)
		{
			i = iterator;
			return ;
		}
		
		/// <summary> Enumeration method that maps to Iterator.hasNext()</summary>
		public bool hasMoreElements()
		{
			return i.MoveNext();
		}
		
		/// <summary> Enumeration method that maps to Iterator.next()</summary>
		public System.Object nextElement()
		{
			return i.Current;
		}
	}
}
