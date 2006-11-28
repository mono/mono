// 
// System.Web.Services.Description.PortTypeCollection.cs
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
	public sealed class PortTypeCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal PortTypeCollection (ServiceDescription serviceDescription)
			: base (serviceDescription)
		{
		}

		#endregion // Constructors

		#region Properties

		public PortType this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (PortType) List[index]; 
			}
                        set { List [index] = value; }
		}

		public PortType this [string name] {
			get { 
				int index = IndexOf ((PortType) Table[name]);
				if (index >= 0)
					return this[index]; 
				return null;
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (PortType portType) 
		{
			Insert (Count, portType);	
			return (Count - 1);
		}

		public bool Contains (PortType portType)
		{
			return List.Contains (portType);
		}

		public void CopyTo (PortType[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is PortType))
				throw new InvalidCastException ();
			return ((PortType) value).Name;
		}

		public int IndexOf (PortType portType)
		{
			return List.IndexOf (portType);
		}

		public void Insert (int index, PortType portType)
		{
			List.Insert (index, portType);
		}
	
		public void Remove (PortType portType)
		{
			List.Remove (portType);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((PortType) value).SetParent ((ServiceDescription) parent); 
		}
			
		#endregion // Methods
	}
}
