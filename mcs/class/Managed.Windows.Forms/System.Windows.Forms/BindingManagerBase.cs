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
//	Peter Bartok	pbartok@novell.com
//	Jackson Harper	jackson@ximian.com
//


// NOT COMPLETE

using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms {
	public abstract class BindingManagerBase {
		private BindingsCollection	bindings;
		private bool pulling_data;

		private static int count = 0;
		private int id = count++;

#region Public Constructors
		public BindingManagerBase() {
		}
		#endregion	// Public Constructors

		#region Protected Instance Fields
		protected EventHandler onCurrentChangedHandler;
		protected EventHandler onPositionChangedHandler;
		#endregion	// Protected Instance Fields

		#region Public Instance Properties
		public BindingsCollection Bindings {
			get {
				if (bindings == null) {
					bindings = new BindingsCollection ();
				}
				return bindings;
			}
		}

		public abstract int Count {
			get;
		}

		public abstract object Current {
			get;
		}

		public abstract int Position {
			get; set;
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public abstract void AddNew();

		public abstract void CancelCurrentEdit();

		public abstract void EndCurrentEdit();

		public abstract PropertyDescriptorCollection GetItemProperties();

		public abstract void RemoveAt(int index);

		public abstract void ResumeBinding();

		public abstract void SuspendBinding();
		#endregion	// Public Instance Methods

                internal abstract bool IsSuspended {
                        get;
                }

		#region Protected Instance Methods
		[MonoTODO]
		protected internal virtual PropertyDescriptorCollection GetItemProperties(System.Collections.ArrayList dataSources, System.Collections.ArrayList listAccessors) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual PropertyDescriptorCollection GetItemProperties(Type lisType, int offset, System.Collections.ArrayList dataSources, System.Collections.ArrayList listAccessors) {
			throw new NotImplementedException();
		}

		protected internal abstract string GetListName (System.Collections.ArrayList listAccessors);

		protected internal abstract void OnCurrentChanged (EventArgs e);

		protected void PullData()
		{
			pulling_data = true;
			try {
				UpdateIsBinding ();
				foreach (Binding binding in Bindings)
					binding.PullData ();
			} finally {
				pulling_data = false;
			}
		}

		protected void PushData()
		{
			if (pulling_data)
				return;

			UpdateIsBinding ();
			foreach (Binding binding in Bindings)
				binding.PushData ();
		}

		protected abstract void UpdateIsBinding();
		#endregion	// Protected Instance Methods

		internal void AddBinding (Binding binding)
		{
			if (Bindings.Contains (binding))
				return;
			Bindings.Add (binding);
		}

		#region Events
		public event EventHandler CurrentChanged;
		public event EventHandler PositionChanged;
		#endregion	// Events
	}
}
