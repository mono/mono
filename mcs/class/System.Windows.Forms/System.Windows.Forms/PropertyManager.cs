//
// System.Windows.Forms.PropertyManager
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

//using System.Drawing;
//using System.Drawing.Printing;
using System.ComponentModel;
using System.Collections;
//using System.Windows.Forms.Design;

namespace System.Windows.Forms {

	/// <summary>
	/// Maintains a Binding between an object's property and a data-bound control property.
	/// </summary>

	[MonoTODO]
	public class PropertyManager : BindingManagerBase {

		/*
		#region Fields
		#endregion
		*/
		
		#region Constructor
		[MonoTODO]
		public PropertyManager() 
		{
			
		}
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
		
		[MonoTODO]
		protected internal override void OnCurrentChanged(EventArgs ea) 
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
	}
}
