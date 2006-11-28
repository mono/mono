// 
// System.Web.Services.Description.OperationBindingCollection.cs
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
	public sealed class OperationBindingCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal OperationBindingCollection (Binding binding)
			: base (binding)
		{
		}

		#endregion // Constructors

		#region Properties

		public OperationBinding this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (OperationBinding) List[index]; 
			}
			set { List[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (OperationBinding bindingOperation) 
		{
			Insert (Count, bindingOperation);
			return (Count - 1);
		}

		public bool Contains (OperationBinding bindingOperation)
		{
			return List.Contains (bindingOperation);
		}

		public void CopyTo (OperationBinding[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (OperationBinding bindingOperation)
		{
			return List.IndexOf (bindingOperation);
		}

		public void Insert (int index, OperationBinding bindingOperation)
		{
			List.Insert (index, bindingOperation);
		}
	
		public void Remove (OperationBinding bindingOperation)
		{
			List.Remove (bindingOperation);
		}

		protected override void SetParent (object value, object parent)
		{
			((OperationBinding) value).SetParent ((Binding) parent);
		}
			
		#endregion // Methods
	}
}
