//
// System.Windows.Forms.PropertyManager
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

//
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
