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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Collections;
using System.ComponentModel;


namespace System.Windows.Forms {

	public class ControlBindingsCollection : BindingsCollection {

		private Control control;

		internal ControlBindingsCollection (Control control)
		{
			this.control = control;
		}

		public new void Add (Binding binding)
		{
			AddCore (binding);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, binding));
		}

		public Binding Add (string property_name, object data_source, string data_member)
		{
			Binding res = new Binding (property_name, data_source, data_member);
			Add (res);
			return res;
		}

		protected override void AddCore (Binding binding)
		{
			if (binding == null)
				throw new ArgumentNullException ("dataBinding");

			if (binding.Control != null && binding.Control != control)
				  throw new ArgumentException ("dataBinding belongs to another BindingsCollection");

			binding.SetControl (control);
			base.AddCore (binding);
		}
	}
}

