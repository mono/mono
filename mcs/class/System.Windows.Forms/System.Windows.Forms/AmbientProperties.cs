//
// System.Windows.Forms.AmbientProperties
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc 2002
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

		//[Serializable]
		//[ClassInterface(ClassInterfaceType.AutoDual)]
		~AmbientProperties(){
		}

		// --- (public) Properties ---

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this AmbientProperties and another object.
		/// </remarks>
		
		public override bool Equals (object obj) {
			if (!(obj is AmbientProperties))
				return false;

			return (this == (AmbientProperties) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode () {
			unchecked{//FIXME Add out proprities to the hash
				return base.GetHashCode();
			}
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the AmbientProperties as a string.
		/// </remarks>
		
		//inherited
		//public override string ToString () 
		//{
		//	//FIXME add our proprities to ToString
		//	return base.ToString();// String.Format ("[{0},{1},{2}]", bindingpath, bindingfield, bindingmember);
		//}

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
