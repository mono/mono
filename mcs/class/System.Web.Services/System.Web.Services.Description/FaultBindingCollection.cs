// 
// System.Web.Services.Description.FaultBindingCollection.cs
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
	public sealed class FaultBindingCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal FaultBindingCollection (OperationBinding operationBinding) 
			: base (operationBinding)
		{
		}

		#endregion // Constructors

		#region Properties

		public FaultBinding this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (FaultBinding) List[index]; 
			}
                        set { List [index] = value; }
		}

		public FaultBinding this [string name] {
			get { return this [IndexOf ((FaultBinding) Table[name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (FaultBinding bindingOperationFault) 
		{
			Insert (Count, bindingOperationFault);
			return (Count - 1);
		}

		public bool Contains (FaultBinding bindingOperationFault)
		{
			return List.Contains (bindingOperationFault);
		}

		public void CopyTo (FaultBinding[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is FaultBinding))
				throw new InvalidCastException ();

			return ((FaultBinding) value).Name;
		}

		public int IndexOf (FaultBinding bindingOperationFault)
		{
			return List.IndexOf (bindingOperationFault);
		}

		public void Insert (int index, FaultBinding bindingOperationFault)
		{
			List.Insert (index, bindingOperationFault);
		}
	
		public void Remove (FaultBinding bindingOperationFault)
		{
			List.Remove (bindingOperationFault);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((FaultBinding) value).SetParent ((OperationBinding) parent);	
		}
			
		#endregion // Methods
	}
}
