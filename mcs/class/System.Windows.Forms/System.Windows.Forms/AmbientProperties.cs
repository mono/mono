//
// System.Windows.Forms.AmbientProperties
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc 2002/3
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
