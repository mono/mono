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
// Novell.Directory.Ldap.MessageVector.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap
{
	
	/// <summary> The <code>MessageVector</code> class implements additional semantics
	/// to Vector needed for handling messages.
	/// </summary>
	/* package */
	class MessageVector:System.Collections.ArrayList
	{
		/// <summary>Returns an array containing all of the elements in this MessageVector.
		/// The elements returned are in the same order in the array as in the
		/// Vector.  The contents of the vector are cleared.
		/// 
		/// </summary>
		/// <returns> the array containing all of the elements.
		/// </returns>
		virtual internal System.Object[] ObjectArray
		{
			/* package */
			
			get
			{
				lock (this)
				{
					System.Object[] results = new System.Object[Count];
					Array.Copy((System.Array) ToArray(), 0, (System.Array) results, 0, Count);
					for (int i = 0; i < Count; i++)
					{
						ToArray()[i] = null;
					}
//					Count = 0;
					return results;
				}
			}
			
		}
		/* package */
		internal MessageVector(int cap, int incr):base(cap)
		{
			return ;
		}
		
		/// <summary> Finds the Message object with the given MsgID, and returns the Message
		/// object. It finds the object and returns it in an atomic operation.
		/// 
		/// </summary>
		/// <param name="msgId">The msgId of the Message object to return
		/// 
		/// </param>
		/// <returns> The Message object corresponding to this MsgId.
		/// 
		/// @throws NoSuchFieldException when no object with the corresponding
		/// value for the MsgId field can be found.
		/// </returns>
		/* package */
		internal Message findMessageById(int msgId)
		{
			lock (this)
			{
				Message msg = null;
				for (int i = 0; i < Count; i++)
				{
					if ((msg = (Message) ToArray()[i]) == null)
					{
						throw new System.FieldAccessException();
					}
					if (msg.MessageID == msgId)
					{
						return msg;
					}
				}
				throw new System.FieldAccessException();
			}
		}
	}
}
