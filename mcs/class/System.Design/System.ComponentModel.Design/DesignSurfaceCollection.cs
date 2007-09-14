//
// System.ComponentModel.Design.DesignSurfaceCollection
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006 Ivan N. Zlatev

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

#if NET_2_0

using System;
using System.ComponentModel;
using System.Collections;

namespace System.ComponentModel.Design
{
	// A read-only collection of design surfaces.
	// A wrapper around a DesignerCollection, which get's the DesignSurface for the IDesignerHost
	// on the fly.
	//
	public sealed class DesignSurfaceCollection : ICollection, IEnumerable
	{
		
		private class DesignSurfaceEnumerator : IEnumerator
		{
			IEnumerator _designerCollectionEnumerator;
			
			public DesignSurfaceEnumerator (IEnumerator designerCollectionEnumerator)
			{
				_designerCollectionEnumerator = designerCollectionEnumerator;
			}

			public bool MoveNext ()
			{
				return _designerCollectionEnumerator.MoveNext ();
			}

			public void Reset ()
			{
				_designerCollectionEnumerator.Reset ();
			}

			public object Current {
				get {
					IDesignerHost designer = (IDesignerHost) _designerCollectionEnumerator.Current;
					DesignSurface surface = designer.GetService (typeof (DesignSurface)) as DesignSurface;
					if (surface == null)
						throw new NotSupportedException ();
					
					return surface;
				}
			}
			
		} // DesignSurfaceEnumerator

		
		private DesignerCollection _designers;
		
		internal DesignSurfaceCollection (DesignerCollection designers)
		{
			if (designers == null)
				designers = new DesignerCollection (null);

			_designers = designers;
		}

		public int Count {
			get { return _designers.Count; }
		}

		public DesignSurface this[int index] {
			get {
				IDesignerHost designer = _designers[index];
				DesignSurface surface = designer.GetService (typeof (DesignSurface)) as DesignSurface;
				if (surface == null)
					throw new NotSupportedException ();

				return surface;
			}
		}

		public void CopyTo (DesignSurface[] array, int index)
		{
			((ICollection) this).CopyTo (array, index);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			foreach (DesignSurface surface in this) {
				array.SetValue (surface, index);
				index++;
			}
		}
		
		public IEnumerator GetEnumerator ()
		{
			return new DesignSurfaceEnumerator (_designers.GetEnumerator ());
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		int ICollection.Count {
			get { return this.Count; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return null; }
		}
		
	}
	
}
#endif
