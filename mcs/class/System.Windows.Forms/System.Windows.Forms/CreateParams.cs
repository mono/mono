//
// System.Windows.Forms.CreateParams.cs
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
	/// Encapsulates the information needed when creating a control.
	/// </summary>
	
	public class CreateParams {

		#region Fields
		private string caption;
		private string className;
		private int classStyle;
		private int exStyle;
		private int height;
		private object param;
		private IntPtr parent;
		private int style;
		private int width;
		private int x;
		private int y;
		#endregion
		
		#region Constructors
		public CreateParams() 
		{
		}
		#endregion
		
		#region Properties
		public string Caption {
			get { return caption; }
			set { caption = value; }
		}
		
		public string ClassName {
			get { return className; }
			set { className = value; }
		}
		
		public int ClassStyle {
			get { return classStyle; }
			set { classStyle = value; }
		}
		
		public int ExStyle {
			get { return exStyle; }
			set { exStyle = value; }
		}
		
		public int Height {
			get { return height; }
			set { height = value; }
		}
		
		public object Param {
			get { return param; }
			set { param = value; }
		}
		
		public IntPtr Parent {
			get { return parent; }
			set { parent = value; }
		}
		
		public int Style {
			get { return style; }
			set { style = value; }
		}
		
		public int Width {
			get { return width; }
			set { width = value; }
		}
		
		public int X {
			get { return x; }
			set { x = value; }
		}
		
		public int Y {
			get { return y; }
			set { y = value; }
		}
		#endregion
		
		#region Methods
		[MonoTODO]
		public override string ToString() 
		{
			//FIXME:
			return base.ToString();
		}
		#endregion
		
	}
}
