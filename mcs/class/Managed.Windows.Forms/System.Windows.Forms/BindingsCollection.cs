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
//      Jackson Harper  jackson@ximian.com
//


// COMPLETE

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {
	[DefaultEvent("CollectionChanged")]
	public class BindingsCollection : BaseCollection {

#region Public Constructors
		internal BindingsCollection ()
		{
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public override int Count {
			get {
				return base.Count;
			}
		}

		public Binding this[int index] {
			get {
				return (Binding)(base.List[index]);
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override ArrayList List {
			get {
				return base.List;
			}
		}
		#endregion	// Protected Instance Properties

		#region Protected Instance Methods
		protected internal void Add(Binding binding) {
			AddCore(binding);
		}

		protected virtual void AddCore (Binding dataBinding) 
		{
			CollectionChangeEventArgs args = new CollectionChangeEventArgs (CollectionChangeAction.Add, dataBinding);
			OnCollectionChanging (args);
			base.List.Add(dataBinding);
			OnCollectionChanged (args);
		}

		protected internal void Clear() {
			ClearCore();
		}

		protected virtual void ClearCore() 
		{
			CollectionChangeEventArgs args = new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null);
			OnCollectionChanging (args);
			base.List.Clear();
			OnCollectionChanged (args);
		}

		protected virtual void OnCollectionChanged(System.ComponentModel.CollectionChangeEventArgs ccevent) {
			if (CollectionChanged!=null) CollectionChanged(this, ccevent);
		}

		protected virtual void OnCollectionChanging (CollectionChangeEventArgs e)
		{
			if (CollectionChanging != null)
				CollectionChanging (this, e);
		}

		protected internal void Remove(Binding binding) {
			RemoveCore(binding);
		}

		protected internal void RemoveAt(int index) {
			base.List.RemoveAt(index);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, base.List));
		}

		protected virtual void RemoveCore(Binding dataBinding) 
		{
			CollectionChangeEventArgs args = new CollectionChangeEventArgs(CollectionChangeAction.Remove, dataBinding);
			OnCollectionChanging (args);
			base.List.Remove(dataBinding);
			OnCollectionChanged (args);
		}

		protected internal bool ShouldSerializeMyAll() {
			if (this.Count>0) {
				return(true);
			} else {
				return(false);
			}
		}
		#endregion	// Public Instance Methods

		internal bool Contains (Binding binding)
		{
			return List.Contains (binding);
		}

		#region Events
		public event CollectionChangeEventHandler	CollectionChanged;
		public event CollectionChangeEventHandler	CollectionChanging;
		#endregion	// Events
	}
}
