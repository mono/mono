// 
// System.Web.Services.Description.ServiceCollection.cs
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

using System.Web.Services;

namespace System.Web.Services.Description {
	public sealed class ServiceCollection : ServiceDescriptionBaseCollection {
		
		#region Constructors
	
		internal ServiceCollection (ServiceDescription serviceDescription)
			: base (serviceDescription)
		{
		}

		#endregion // Constructors

		#region Properties

		public Service this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (Service) List[index]; 
			}
			set { List [index] = value; }
		}

		public Service this [string name] {
			get { 
				int index = IndexOf ((Service) Table[name]);
				if (index >= 0)
					return this[index]; 
				return null;
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (Service service) 
		{
			Insert (Count, service);
			return (Count - 1);
		}

		public bool Contains (Service service)
		{
			return List.Contains (service);
		}

		public void CopyTo (Service[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is Service))
				throw new InvalidCastException ();

			return ((Service) value).Name;
		}

		public int IndexOf (Service service)
		{
			return List.IndexOf (service);
		}

		public void Insert (int index, Service service)
		{
			List.Insert (index, service);
		}
	
		public void Remove (Service service)
		{
			List.Remove (service);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((Service) value).SetParent ((ServiceDescription) parent);
		}
			
		#endregion // Methods
	}
}
