//
// System.Windows.Forms.BindingManagerBase.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//

using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms {
	//Compact Framework. Everything execpt suspend and resume binding needed for CE.
	[MonoTODO]
	public abstract class BindingManagerBase {


		/// <summary>
		/// Manages all Binding objects that are bound to the same data source and data member. This class is abstract.
		/// </summary>

		// --- Constructor
		[MonoTODO]
		public BindingManagerBase ()
		{
		}
		
		//
		// --- Fields
		protected EventHandler onCurrentChangedHandler;
		protected EventHandler onPositionChangedHandler;


		//
		// --- Public Properties
		[MonoTODO]
		public BindingsCollection Bindings  {

			get { throw new NotImplementedException (); }
		}
		
		public abstract int Count  {

			get;
		}

		public abstract object Current  {

			get;
		}

		public abstract int Position  {

			get;
			set;
		}

		//
		// --- Methods
		public abstract void AddNew();

		public abstract void CancelCurrentEdit();

		public abstract void EndCurrentEdit();

		public abstract PropertyDescriptorCollection GetItemProperties ();

		[MonoTODO]
		protected internal virtual PropertyDescriptorCollection GetItemProperties (ArrayList dataSources,ArrayList listAccessors)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual PropertyDescriptorCollection GetItemProperties (Type listType,int offset,ArrayList dataSources,ArrayList listAccessors)
		{
			throw new NotImplementedException ();
		}

		//protected abstract string GetListName(ArrayList listAccessors);
		protected internal abstract string GetListName(ArrayList listAccessors);

		protected internal abstract void OnCurrentChanged(EventArgs e); 

		[MonoTODO]
		protected void PullData()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void PushData()
		{
			throw new NotImplementedException ();
		}

		public abstract void RemoveAt(int index);

		public abstract void ResumeBinding();
		public abstract void SuspendBinding();

		protected abstract void UpdateIsBinding();


		//
		// --- Public Events

		public event EventHandler CurrentChanged;
		public event EventHandler PositionChanged;
	}
}
