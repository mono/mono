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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// COMPLETE

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {
	public class CreateParams {
		#region Local Variables
		private string	caption;
		private string	class_name;
		private int	class_style;
		private int	ex_style;
		private int	x;
		private int	y;
		private int	height;
		private int	width;
		private int	style;
		private object	param;
		private IntPtr	parent;
		internal Menu	menu;
		internal Control	control;
		#endregion 	// Local variables

		#region Public Constructors
		public CreateParams() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public string Caption {
			get { return caption; }
			set { caption = value; }
		}

		public string ClassName {
			get { return class_name; }
			set { class_name = value; }
		}

		public int ClassStyle {
			get { return class_style; }
			set { class_style = value; }
		}

		public int ExStyle {
			get { return ex_style; }
			set { ex_style = value; }
		}

		public int X {
			get { return x; }
			set { x = value; }
		}

		public int Y {
			get { return y; }
			set { y = value; }
		}

		public int Width {
			get { return width; }
			set { width = value; }
		}

		public int Height {
			get { return height; }
			set { height = value; }
		}

		public int Style {
			get { return style; }
			set { style = value; }
		}

		public object Param {
			get { return param; }
			set { param = value; }
		}

		public IntPtr Parent {
			get { return parent; }
			set { parent = value; }
		}
		#endregion	// Public Instance Properties

		#region Internal Instance Methods
		internal bool IsSet (WindowStyles Style) {
			return (this.style & (int) Style) == (int) Style;
		}
		
		internal bool IsSet (WindowExStyles ExStyle) {
			return (this.ex_style & (int) ExStyle) == (int) ExStyle;
		}
		
		internal static bool IsSet (WindowExStyles ExStyle, WindowExStyles Option) {
			return (Option & ExStyle) == Option;
		}
		
		internal static bool IsSet (WindowStyles Style, WindowStyles Option) {
			return (Option & Style) == Option;
		}
		
		internal bool HasWindowManager {
			get {
				if (control == null)
					return false;
				
				Form form = control as Form;
				
				if (form == null)
					return false;
				
				return form.window_manager != null;
			}
		}
		internal WindowExStyles WindowExStyle {
			get {
				return (WindowExStyles) ex_style;
			}
			set
			{
				ex_style = (int)value;
			}
		}
		
		internal WindowStyles WindowStyle {
			get {
				return (WindowStyles) style;
			}
			set {
				style = (int) value;
			}
		}
		#endregion

		#region Public Instance Methods
		public override string ToString() {
			return string.Format ("CreateParams {{'{0}', '{1}', 0x{2:X}, 0x{3:X}, {{{4}, {5}, {6}, {7}}}}}", 
					class_name, caption, class_style, ex_style, x, y, width, height);
 		}
		#endregion	// Public Instance Methods

	}
}
