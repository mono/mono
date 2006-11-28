// 
// System.Web.Services.Description.MessagePartCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Web.Services.Description {
	public sealed class MessagePartCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal MessagePartCollection (Message message)
			: base (message)
		{
		}	

		#endregion

		#region Properties

		public MessagePart this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (MessagePart) List[index]; 
			}
                        set { List [index] = value; }
		}

		public MessagePart this [string name] {
			get { return this [IndexOf ((MessagePart) Table[name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (MessagePart messagePart) 
		{
			Insert (Count, messagePart);
			return (Count - 1);
		}

		public bool Contains (MessagePart messagePart)
		{
			return List.Contains (messagePart);
		}

		public void CopyTo (MessagePart[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is MessagePart))
				throw new InvalidCastException ();
			return ((MessagePart) value).Name;
		}

		public int IndexOf (MessagePart messagePart)
		{
			return List.IndexOf (messagePart);
		}

		public void Insert (int index, MessagePart messagePart)
		{
			List.Insert (index, messagePart);
		}
	
		public void Remove (MessagePart messagePart)
		{
			List.Remove (messagePart);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((MessagePart) value).SetParent ((Message) parent);
		}
			
		#endregion // Methods
	}
}
