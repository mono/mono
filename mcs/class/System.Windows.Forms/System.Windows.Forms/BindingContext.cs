//
// System.Windows.Forms.BindingContext.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc
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
namespace System.Windows.Forms {

	public class BindingContext {

		/// <summary>
		/// Manages the collection of BindingManagerBase objects for any object that inherits from the Control class.
		/// </summary>

		//private 
		// --- Constructor
		[MonoTODO]
		public BindingContext ()
		{
			//
		}

		//
		// --- Public Properties
		// Following properties not stubbed out, because they are only supporting internal .NET Framework infrastructure.
		[MonoTODO]
		public bool IsReadOnly {
			get {
				throw new NotImplementedException ();
			}
		}
	
		[MonoTODO]
		public BindingManagerBase this[object dataSource]  {

			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public BindingManagerBase this[object dataSource,string dataMember]  {

			get { throw new NotImplementedException (); }
		}

		//
		// --- Methods
		// Following methods not stubbed out, because they are only supporting internal .NET Framework infrastructure.
		[MonoTODO]
		protected virtual void AddCore(object dataSource,BindingManagerBase listManager){
		}

		[MonoTODO]
		protected virtual void ClearCore(){
		}

		// - void ICollection.CopyTo(Array ar,int index)
		// - IEnumerator IEnumerable.GetEnumerator()

		[MonoTODO]
		protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent){
		}

		[MonoTODO]
		protected virtual void RemoveCore(object dataSource){
		}

		[MonoTODO]
		protected internal void Add(object dataSource, BindingManagerBase listManager)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void Clear()
		{
			//FIXME:
		}

		[MonoTODO]
		public bool Contains(object dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(object dataSource,string dataMember)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void Remove(object dataSource)
		{
			//FIXME:
		}

		//
		// --- Public Events
		// Following events not stubbed out, because they are only supporting internal .NET Framework infrastructure
		public event CollectionChangeEventHandler CollectionChanged;
	}
}
