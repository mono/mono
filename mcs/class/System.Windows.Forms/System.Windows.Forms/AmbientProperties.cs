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

namespace System.Windows.Forms
{
	/// <summary>
	/// Provides ambient property values to top-level controls.
	/// </summary>
	
	public sealed class AmbientProperties
	{
		
		// --- Constructor ---
		public AmbientProperties() {
			//
		}

		[Serializable]
		[ClassInterface(ClassInterfaceType.AutoDual)]
		~AmbientProperties();

		// --- (public) Properties ---
		Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this AmbientProperties and another object.
		/// </remarks>
		
		public override bool Equals (object o) {
			if (!(o is AmbientProperties))
				return false;

			return (this == (AmbientProperties) o);
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
		
		public override string ToString () {
			//FIXME add our proprities to ToString
			return base.ToString();// String.Format ("[{0},{1},{2}]", bindingpath, bindingfield, bindingmember);
		}

		Cursor Cursor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	
		public Font Font {
			get {
				throw new NotImplementedException (); 
			}
			set {
				throw new NotImplementedException (); 
			}
		}
	
		public Color Forecolor {
			get { 
				throw new NotImplementedException (); 
			}
			set {
				throw new NotImplementedException (); 
			}
		}
	}
}
