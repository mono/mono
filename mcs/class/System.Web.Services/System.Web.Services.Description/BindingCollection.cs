// 
// System.Web.Services.Description.BindingCollection.cs
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
	public sealed class BindingCollection : ServiceDescriptionBaseCollection {
		
		#region Fields

		ServiceDescription serviceDescription;

		#endregion // Fields

		#region Constructors
	
		internal BindingCollection (ServiceDescription serviceDescription) 
			: base (serviceDescription)
		{
		}

		#endregion // Constructors

		#region Properties

		public Binding this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (Binding) List[index]; 
			}
                        set { List [index] = value; }
		}

		public Binding this [string name] {
			get { return (Binding) Table[name]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Binding binding) 
		{
			Insert (Count, binding);
			return (Count - 1);
		}

		public bool Contains (Binding binding)
		{
			return List.Contains (binding);
		}

		public void CopyTo (Binding[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is Binding))
				throw new InvalidCastException ();
			return ((Binding) value).Name;
		}

		public int IndexOf (Binding binding)
		{
			return List.IndexOf (binding);
		}

		public void Insert (int index, Binding binding)
		{
			List.Insert (index, binding);
		}
	
		public void Remove (Binding binding)
		{
			List.Remove (binding);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((Binding) value).SetParent ((ServiceDescription) parent);
		}
			
		#endregion // Methods
	}
}
