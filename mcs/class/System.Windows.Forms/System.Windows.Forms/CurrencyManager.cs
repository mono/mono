//
// System.Windows.Forms.CurrencyManager.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
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

using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms {

	/// <summary>
	/// Manages a list of Binding objects.
	/// </summary>
	[MonoTODO]
	public class CurrencyManager : BindingManagerBase {

		private CurrencyManager(){//For signiture compatablity. Prevents the auto creation of public constructor
		}

		#region Fields
		protected Type finalType;
		protected int listposition;
		internal int count = 0;
		internal object current = null;
		internal int position = 0;
		#endregion

		#region Properties
		[MonoTODO]
		public override int Count {
			get {
				return count;
			}
		}

		[MonoTODO]
		public override object Current {
			get {
				//FIXME:
				return current;
			}
		}
		
		[MonoTODO]
		public IList List {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override int Position {

			get {
				//FIXME:
				return position;
			}
			set {
				//FIXME:
				position = value;
			}
		}
		#endregion

		#region Methods
		[MonoTODO]
		public override void AddNew() 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void CancelCurrentEdit() 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void CheckEmpty() 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void EndCurrentEdit() 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override PropertyDescriptorCollection GetItemProperties() 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override string GetListName(ArrayList listAccessors) 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		/// <methods for events>
		[MonoTODO]
		protected internal override void OnCurrentChanged(EventArgs e) 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnItemChanged(ItemChangedEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		// this method only supports .NET framework
		[MonoTODO]
		protected virtual void OnPositionChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		/// </methods for events>
		
		[MonoTODO]
		public void Refresh() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void RemoveAt(int index) 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void ResumeBinding() 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void SuspendBinding() 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void UpdateIsBinding() 
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		#endregion

		#region Events
		[MonoTODO]
		public event ItemChangedEventHandler ItemChanged;
		public event EventHandler MetaDataChanged; // .NET V1.1 Beta
		#endregion
	}
}
