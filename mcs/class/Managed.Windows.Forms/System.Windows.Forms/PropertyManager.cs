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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jackson Harper	jackson@ximian.com

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class PropertyManager : BindingManagerBase
	{
		private object current;

		internal PropertyManager (object data_source)
		{

		}

		public override object Current {
			get { return current; }
		}

		public override int Position {
			get { return 0; }
			set { }
		}

		public override int Count {
			get { return 0; }
		}

		public override void AddNew () {
		}

		public override void CancelCurrentEdit () {
		}

		public override void EndCurrentEdit () {
		}

		public override PropertyDescriptorCollection GetItemProperties ()
		{
			throw new NotImplementedException ();
		}

		public override void RemoveAt (int idx) {
		}

		public override void ResumeBinding () {
		}

		public override void SuspendBinding () {
		}

		protected internal override string GetListName (ArrayList list) {
			throw new NotImplementedException ();
		}

		protected override void UpdateIsBinding () {
		}

		protected internal override void OnCurrentChanged (EventArgs e) {
			if (onCurrentChangedHandler != null) {
				onCurrentChangedHandler (this, e);
			}
		}
	}
}
