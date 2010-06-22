//
// System.Web.UI.WebControls.EmbeddedMailObjectsCollection.cs
//
// Authors:
//	Igor Zelmanovich (igorz@mainsoft.com)
//
// (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace System.Web.UI.WebControls
{
	public sealed class EmbeddedMailObjectsCollection : CollectionBase
	{
		[MonoTODO("Not implemented")]
		public EmbeddedMailObject this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO("Not implemented")]
		public int Add (EmbeddedMailObject value) {
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public bool Contains (EmbeddedMailObject value) {
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public void CopyTo (EmbeddedMailObject [] array, int index) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public int IndexOf (EmbeddedMailObject value) {
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public void Insert (int index, EmbeddedMailObject value) {
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		protected override void OnValidate (object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public void Remove (EmbeddedMailObject value) {
			throw new NotImplementedException ();
		}
	}
}

