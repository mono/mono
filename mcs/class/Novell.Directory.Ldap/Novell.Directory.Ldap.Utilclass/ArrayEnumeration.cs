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
// Novell.Directory.Ldap.Utilclass.ArrayEnumeration.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	public class ArrayEnumeration : System.Collections.IEnumerator
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
		private System.Object[] eArray;
		private int index = 0;
		/// <summary> Constructor to create the Enumeration
		/// 
		/// </summary>
		/// <param name="eArray">the array to use for the Enumeration
		/// </param>
		public ArrayEnumeration(System.Object[] eArray)
		{
			this.eArray = eArray;
		}
		
		public bool hasMoreElements()
		{
			if (eArray == null)
				return false;
			return (index < eArray.Length);
		}
		
		public System.Object nextElement()
		{
			if ((eArray == null) || (index >= eArray.Length))
			{
				throw new System.ArgumentOutOfRangeException();
			}
			return eArray[index++];
		}
	}
}
