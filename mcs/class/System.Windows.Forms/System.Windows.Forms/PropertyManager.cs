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
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public override object Current {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public override int Position {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		#endregion
		
		#region Methods
		[MonoTODO]
		public override void AddNew() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public override void CancelCurrentEdit() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public override void EndCurrentEdit() 
		{
			//FIXME:
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
			//FIXME:
		}
		
		[MonoTODO]
		public override void RemoveAt(int index) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public override void ResumeBinding() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public override void SuspendBinding() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void UpdateIsBinding() 
		{
			//FIXME:
		}
		#endregion
	}
}
