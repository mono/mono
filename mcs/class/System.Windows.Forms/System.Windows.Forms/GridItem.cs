//
// System.Windows.Forms.GridItem.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
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

	// <summary>
	// </summary>

    public abstract class GridItem {
		bool expandable;
		bool expanded;
		//
		//  --- Public Properties
		//
		public virtual bool Expandable {
			get {
				return expandable;
			}
		}
		public virtual bool Expanded {
			get {
				return expanded;
			}
			set {
				expanded = value;
			}
		}


		public abstract GridItemCollection GridItems {
			get;
		}
		public abstract GridItemType GridItemType {
			get;
		}
		public abstract string Label {
			get;
		}
		public abstract GridItem Parent {
			get;
		}
		public abstract PropertyDescriptor PropertyDescriptor {
			get;
		}
		public abstract object Value {
			get;
		}

		public abstract bool Select ();
		//
		// --- Protected Constructor
		//
		[MonoTODO]
		protected GridItem()
		{
			
		}
	 }
}
