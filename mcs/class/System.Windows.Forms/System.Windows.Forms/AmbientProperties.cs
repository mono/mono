//
// System.Windows.Forms.AmbientProperties
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc 2002/3
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
using System.Runtime.InteropServices;

using System;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides ambient property values to top-level controls.
	/// </summary>
	
	public sealed class AmbientProperties {
		Cursor cursor;
		Font font;
		Color backColor;
		Color foreColor;
		// --- Constructor ---
		public AmbientProperties() {
			cursor = null;
			font = null;
			backColor = Color.Empty;
			foreColor = Color.Empty;
		}

		// --- (public) Properties ---

		public Cursor Cursor {
			get {
				//if set, use our value, if not, search site for it.
				if(cursor != null){
					//This is correct
					return cursor;
				}
				else{
					//This needs to find cursor from the system
					//If it cannot, return default
					return cursor;//FIXME: get value from system
				}
			}
			set {
				cursor = value;
			}
		}
	
		public Font Font {
			get {
				if(font != null){
					return  font;
				}
				else{//try to get font from system
					return font;//FIXME: get value from system
				}
			}
			set {
				font = value; 
			}
		}
	
		public Color ForeColor {
			get { 
				if(foreColor != Color.Empty){
					return foreColor;
				}
				else{
					//FIXME: return system color if possible
					return foreColor;
				}
			}
			set {
				foreColor = value;
			}
		}
		public Color BackColor {
			get { 
				if(backColor != Color.Empty){
					return backColor;
				}
				else{
					//FIXME: return system color if possible
					return backColor;
				}
			}
			set {
				backColor = value; 
			}
		}
	}
}
