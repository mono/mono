// 
// System.Web.Services.Protocols.SoapHeaderCollection.cs
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

using System.Collections;

namespace System.Web.Services.Protocols {
	public class SoapHeaderCollection : CollectionBase {

		#region Constructors

		public SoapHeaderCollection ()
		{
		}

		#endregion

		#region Properties

		public SoapHeader this [int index] {
			get { return (SoapHeader) List[index]; }
			set { List[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (SoapHeader header)
		{
			Insert (Count, header);
			return (Count - 1);
		}

		public bool Contains (SoapHeader header)
		{
			return List.Contains (header);
		}

		public void CopyTo (SoapHeader[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (SoapHeader header)
		{
			return List.IndexOf (header);
		}

		public void Insert (int index, SoapHeader header)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ();
			List.Insert (index, header);
		}

		public void Remove (SoapHeader header)
		{
			List.Remove (header);
		}

		#endregion // Methods
	}
}
