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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//
// $Log: BindingManagerBase.cs,v $
// Revision 1.2  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.1  2004/08/11 21:22:27  pbartok
// - Was checked in with wrong filename
//
//
//

// NOT COMPLETE

using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms {
	public abstract class BindingManagerBase {
		private BindingsCollection	bindings;

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
				if (this.bindings==null) {
					this.bindings=new BindingsCollection();
				}
				return this.bindings;
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

		#region Protected Instance Methods
		[MonoTODO]
		protected internal virtual PropertyDescriptorCollection GetItemProperties(System.Collections.ArrayList dataSources, System.Collections.ArrayList listAccessors) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual PropertyDescriptorCollection GetItemProperties(Type lisType, int offset, System.Collections.ArrayList dataSources, System.Collections.ArrayList listAccessors) {
			throw new NotImplementedException();
		}

		protected internal abstract string GetListName(System.Collections.ArrayList listAccessors);

		protected internal abstract void OnCurrentChanged(EventArgs e);

		[MonoTODO]
		protected void PullData() {
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected void PushData() {
			throw new NotImplementedException();
		}

		protected abstract void UpdateIsBinding();
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler CurrentChanged;
		public event EventHandler PositionChanged;
		#endregion	// Events
	}
}
