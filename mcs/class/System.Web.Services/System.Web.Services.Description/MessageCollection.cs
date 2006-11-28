// 
// System.Web.Services.Description.MessageCollection.cs
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
	public sealed class MessageCollection : ServiceDescriptionBaseCollection {

		#region Constructors
		
		internal MessageCollection (ServiceDescription serviceDescription)
			: base (serviceDescription)
		{
		}

		#endregion

		#region Properties

		public Message this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (Message) List [index]; 
			}
                        set { List [index] = value; }
		}

		public Message this [string name] {
			get { return this [IndexOf ((Message) Table [name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Message message) 
		{
			Insert (Count, message);
			return (Count - 1);
		}

		public bool Contains (Message message)
		{
			return List.Contains (message);
		}

		public void CopyTo (Message[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is Message))
				throw new InvalidCastException ();

			return ((Message) value).Name;
		}

		public int IndexOf (Message message)
		{
			return List.IndexOf (message);
		}

		public void Insert (int index, Message message)
		{
			List.Insert (index, message);
		}
	
		public void Remove (Message message)
		{
			List.Remove (message);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((Message) value).SetParent ((ServiceDescription) parent);
		}
			
		#endregion // Methods
	}
}
