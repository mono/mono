//
// System.Windows.Forms.ControlBindingsCollection.cs
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
