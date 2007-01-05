// 
// System.Web.Services.Description.OperationCollection.cs
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
	public sealed class OperationCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal OperationCollection (PortType portType) 
			: base (portType)
		{
		}

		#endregion // Constructors

		#region Properties

		public Operation this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (Operation) List[index]; 
			}
			set { List[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Operation operation) 
		{
			Insert (Count, operation);
			return (Count - 1);
		}

		public bool Contains (Operation operation)
		{
			return List.Contains (operation);
		}

		public void CopyTo (Operation[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		internal Operation Find (string name)
		{
			foreach (Operation op in List)
				if (op.Name == name)
					return op;
			return null;
		}

		public int IndexOf (Operation operation)
		{
			return List.IndexOf (operation);
		}

		public void Insert (int index, Operation operation)
		{
			List.Insert (index, operation);
		}
	
		public void Remove (Operation operation)
		{
			List.Remove (operation);
		}

		protected override void SetParent (object value, object parent)
		{
			((Operation) value).SetParent ((PortType) parent);
		}
			
		#endregion // Methods
	}
}
