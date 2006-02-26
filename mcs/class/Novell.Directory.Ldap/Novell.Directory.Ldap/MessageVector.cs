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
	class MessageVector:System.Collections.IList
	{
		private readonly System.Collections.ArrayList _innerList;
		/// <summary>Returns an array containing all of the elements in this MessageVector.
		/// The elements returned are in the same order in the array as in the
		/// Vector.  The contents of the vector are cleared.
		/// 
		/// </summary>
		/// <returns> the array containing all of the elements.
		/// </returns>
		internal System.Object[] ObjectArray
		{
			/* package */
			
			get
			{
				lock (this.SyncRoot)
				{
					System.Object[] results = ToArray();
					Clear();
					return results;
				}
			}
			
		}
		/* package */
		internal MessageVector(int cap, int incr)
		{
			_innerList = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(cap));
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
			lock (this.SyncRoot)
			{
				Message msg = null;
				for (int i = 0; i < Count; i++)
				{
					if ((msg = (Message) this[i]) == null)
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

		#region ArrayList members
		public object[] ToArray()
		{
			return _innerList.ToArray();
		}
		#endregion

		#region IList Members

		public int Add(object value)
		{
			return _innerList.Add(value);
		}

		public void Clear()
		{
			_innerList.Clear();
		}

		public bool Contains(object value)
		{
			return _innerList.Contains(value);
		}

		public int IndexOf(object value)
		{
			return _innerList.IndexOf(value);
		}

		public void Insert(int index, object value)
		{
			_innerList.Insert(index, value);
		}

		public bool IsFixedSize
		{
			get { return _innerList.IsFixedSize; }
		}

		public bool IsReadOnly
		{
			get { return _innerList.IsReadOnly; }
		}

		public void Remove(object value)
		{
			_innerList.Remove(value);
		}

		public void RemoveAt(int index)
		{
			_innerList.RemoveAt(index);
		}

		public object this[int index]
		{
			get
			{
				return _innerList[index];
			}
			set
			{
				_innerList[index] = value;
			}
		}

		#endregion

		#region ICollection Members

		public void CopyTo(Array array, int index)
		{
			_innerList.CopyTo(array, index);
		}

		public int Count
		{
			get { return _innerList.Count; }
		}

		public bool IsSynchronized
		{
			get { return _innerList.IsSynchronized; }
		}

		public object SyncRoot
		{
			get { return _innerList.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		public System.Collections.IEnumerator GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}

		#endregion
	}
}
