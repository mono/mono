//
// System.Windows.Forms.Cursors.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides a collection of Cursor objects for use by a Windows Forms application.
	/// </summary>

	[MonoTODO]
	public sealed class Cursors {

		#region Properties
		[MonoTODO]
		public static Cursor AppStarting {
			
			get {
//HANDLE LoadImage(
//  HINSTANCE hinst,   // handle to instance // = null
//  LPCTSTR lpszName,  // image to load // = IDC_APPSTARTING
//  UINT uType,        // image type //= IMAGE_CURSOR
//  int cxDesired,     // desired width  // = 0
//  int cyDesired,     // desired height // = 0
//  UINT fuLoad        // load options // = LR_DEFAULTSIZE || ??
//);
				//Cursor cursor = new Cursor(
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor Arrow {
			//  LPCTSTR lpszName,  // image to load // = IDC_ARROW
			get {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor Cross {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor Default {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor Hand {
			get {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor Help {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor HSplit {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor IBeam {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor No {
			get {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor NoMove2D {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor NoMoveHoriz {
			get {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor NoMoveVert {
			get {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor PanEast {
			get { 
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor PanNE {
			get {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor PanNorth {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor PanNW {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor PanSE {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor PanSouth {
			get {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor PanSW {
			get { 
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor PanWest {
			get {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor SizeAll {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor SizeNESW {
			get { 
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor SizeNS {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor SizeNWSE {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor SizeWE {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor UpArrow {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Cursor VSplit {
			get {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static Cursor WaitCursor {
			get { 
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}
