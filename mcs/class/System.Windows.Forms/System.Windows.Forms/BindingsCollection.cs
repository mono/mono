//
// System.Windows.Forms.BindingsCollection.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a collection of Binding objects for a control.
	///
	/// </summary>
	
	[MonoTODO]
	public class BindingsCollection : BaseCollection {

		#region Constructors
		protected internal BindingsCollection () 
		{
		}
		#endregion

		// --- public and protected Properties ---
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
		
		[MonoTODO]
		protected override ArrayList List {
			get {
				return base.List;
 }
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
		
		protected internal void Add(Binding binding) 
		{
			base.List.Add(binding);
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Add,
				base.List
			));
		}
		
		protected internal void Clear() 
		{
			base.List.Clear();
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Refresh,
				base.List
			));
		}

		protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent) 
		{
			if (CollectionChanged != null)
				CollectionChanged(this, ccevent);
		}

		protected internal void Remove(Binding binding) 
		{
			base.List.Remove(binding);
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Remove,
				base.List
			));
		}

		protected internal void RemoveAt(int index) 
		{
			base.List.RemoveAt(index);
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Remove,
				base.List
			));
		}
		
		protected internal bool ShouldSerializeMyAll() 
		{
			if (this.Count>0) return true;
			else return false;
		}
		
		// public events
		public event CollectionChangeEventHandler CollectionChanged;
	}
}
