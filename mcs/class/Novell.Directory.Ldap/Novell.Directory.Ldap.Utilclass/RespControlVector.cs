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
// Novell.Directory.Ldap.Utilclass.RespControlVector.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary> The <code>MessageVector</code> class implements extends the
	/// existing Vector class so that it can be used to maintain a
	/// list of currently registered control responses.
	/// </summary>
	public class RespControlVector:System.Collections.ArrayList
	{
		public RespControlVector(int cap, int incr):base(cap)
		{
			return ;
		}
		
		/// <summary>Inner class defined to create a temporary object to encapsulate
		/// all registration information about a response control.  This class
		/// cannot be used outside this class 
		/// </summary>
		private class RegisteredControl
		{
			private void  InitBlock(RespControlVector enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private RespControlVector enclosingInstance;
			public RespControlVector Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public System.String myOID;
			public System.Type myClass;
			
			public RegisteredControl(RespControlVector enclosingInstance, System.String oid, System.Type controlClass)
			{
				InitBlock(enclosingInstance);
				myOID = oid;
				myClass = controlClass;
			}
		}
		
		/* Adds a control to the current list of registered response controls.
		*
		*/
		public void  registerResponseControl(System.String oid, System.Type controlClass)
		{
			lock (this)
			{
				
				Add(new RegisteredControl(this, oid, controlClass));
			}
		}
		
		/* Searches the list of registered controls for a mathcing control.  We
		* search using the OID string.  If a match is found we return the
		* Class name that was provided to us on registration.
		*/
		public System.Type findResponseControl(System.String searchOID)
		{
			lock (this)
			{
				RegisteredControl ctl = null;
				
				/* loop through the contents of the vector */
				for (int i = 0; i < Count; i++)
				{
					
					/* Get next registered control */
					if ((ctl = (RegisteredControl) this[i]) == null)
					{
						throw new System.FieldAccessException();
					}
					
					/* Does the stored OID match with whate we are looking for */
					if (ctl.myOID.CompareTo(searchOID) == 0)
					{
						
						
						/* Return the class name if we have match */
						return ctl.myClass;
					}
				}
				/* The requested control does not have a registered response class */
				return null;
			}
		}
	}
}
