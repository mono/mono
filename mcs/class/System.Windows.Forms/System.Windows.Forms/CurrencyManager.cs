//
// System.Windows.Forms.CurrencyManager.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//
// (C) 2002 Ximian, Inc
//

using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms {

	/// <summary>
	/// Manages a list of Binding objects.
	/// </summary>
	[MonoTODO]
	public class CurrencyManager : BindingManagerBase {

		#region Fields
		/* uncomment if needed
		protected Type finalType;
		protected int listposition;
		*/
		#endregion



		#region Properties
		[MonoTODO]
		public override int Count {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override object Current {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public IList List {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int Position {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		#endregion




		#region Methods
		[MonoTODO]
		public override void AddNew() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void CancelCurrentEdit() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void CheckEmpty() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void EndCurrentEdit() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override PropertyDescriptorCollection GetItemProperties() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override string GetListName(ArrayList listAccessors) 
		{
			throw new NotImplementedException ();
		}
		
		/// <methods for events>
		[MonoTODO]
		protected internal override void OnCurrentChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnItemChanged(ItemChangedEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		/* this method only supports .NET framework
		[MonoTODO]
		protected virtual void OnPositionChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}*/
		/// </methods for events>
		
		[MonoTODO]
		public void Refresh() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void RemoveAt(int index) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void ResumeBinding() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void SuspendBinding() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void UpdateIsBinding() 
		{
			throw new NotImplementedException ();
		}
		#endregion



		#region Events
		[MonoTODO]
		public event ItemChangedEventHandler ItemChanged {
			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
		#endregion
	}
}
