//
// System.Windows.Forms.ControlBindingsCollection.cs
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
	/// Represents the collection of data bindings for a control.
	/// </summary>
	
	[MonoTODO]
	public class ControlBindingsCollection : BindingsCollection {

		#region Constructors
		protected internal ControlBindingsCollection() : base () 
		{
		}
		#endregion
		
		#region Properties
		[MonoTODO]
		public Control Control {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public Binding this[string propertyName] {
			get { throw new NotImplementedException (); }
		}
		#endregion
		
		#region Methods
		public new void Add(Binding binding) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Binding Add(string propertyName,object dataSource,string dataMember) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public new void Clear() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public new void Remove(Binding binding) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public new void RemoveAt(int index) 
		{
			//FIXME:
		}

		//internal
		[MonoTODO]
		protected override void AddCore(Binding dataBinding) {
		}

		[MonoTODO]
		protected override void ClearCore(){
		}

		[MonoTODO]
		protected override void RemoveCore(Binding dataBinding){
		}

		#endregion
		
	}
}
