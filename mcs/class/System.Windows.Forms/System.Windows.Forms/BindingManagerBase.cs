//
// System.Windows.Forms.BindingManagerBase.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
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
