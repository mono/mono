//
// System.Windows.Forms.BindingsCollection.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms
{
	/// <summary>
	/// Represents a collection of Binding objects for a control.
	///
	/// ToDo note:
	///  - most methods are not implemented
	///  - those few that are implemented need checking
	/// </summary>
	
	[MonoTODO]
	public class BindingsCollection : BaseCollection
	{
		#region Constructors
		protected internal BindingsCollection () {
		}
		#endregion
		
		// --- public and protected Properties ---
		[MonoTODO]
		public override int Count {
			// CHECKME:
			get { return base.Count; }
		}
		
		[MonoTODO]
		public Binding this[int index] {
			// CHECKME:
			get { return (Binding)(base.List[index]); }
		}
		
		[MonoTODO]
		protected override ArrayList List {
			// CHECKME:
			get { return base.List; }
		}
		
		
		
		// --- public Methods ---
		// following internal methods are (will) not be stubbed out:
		// - protected virtual void AddCore(Binding dataBinding);
		// - protected virtual void ClearCore();
		// - protected virtual void RemoveCore(Binding dataBinding);
		// 
		// CollectionChanged event:
		// Though it was not documented, here methods Add and Remove 
		// cause the CollectionChanged event to occur, similarily as Clear.
		// Would be nice if someone checked the exact event behavior of .NET implementation.
		
		[MonoTODO]
		protected internal void Add(Binding binding) {
			// CHECKME:
			base.List.Add(binding);
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Add,
				base.List
			));
		}
		
		[MonoTODO]
		protected internal void Clear() {
			// CHECKME:
			base.List.Clear();
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Refresh,
				base.List
			));
		}

		[MonoTODO]
		protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent) {
			// CHECKME:
			if (CollectionChanged != null)
				CollectionChanged(this, ccevent);
		}

		[MonoTODO]
		protected internal void Remove(Binding binding) {
			// CHECKME:
			base.List.Remove(binding);
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Remove,
				base.List
			));
		}

		[MonoTODO]
		protected internal void RemoveAt(int index) {
			// CHECKME:
			base.List.RemoveAt(index);
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Remove,
				base.List
			));
		}
		
		protected internal bool ShouldSerializeMyAll() {
			if (this.Count>0) return true;
			else return false;
		}
		
		// public events
		public event CollectionChangeEventHandler CollectionChanged;
	}
}
