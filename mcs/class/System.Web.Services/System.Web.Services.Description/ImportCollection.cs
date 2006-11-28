// 
// System.Web.Services.Description.ImportCollection.cs
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
	public sealed class ImportCollection : ServiceDescriptionBaseCollection {

		#region Constructors
		
		internal ImportCollection (ServiceDescription serviceDescription)
			: base (serviceDescription)
		{
		}

		#endregion

		#region Properties

		public Import this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (Import) List[index]; 
			}
			set { List [index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Import import) 
		{
			Insert (Count, import);
			return (Count - 1);
		}

		public bool Contains (Import import)
		{
			return List.Contains (import);
		}

		public void CopyTo (Import[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (Import import)
		{
			return List.IndexOf (import);
		}

		public void Insert (int index, Import import)
		{
			List.Insert (index, import);
		}
	
		public void Remove (Import import)
		{
			List.Remove (import);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((Import) value).SetParent ((ServiceDescription) parent);
		}
			
		#endregion // Methods
	}
}
