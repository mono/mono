//
// System.Windows.Forms.Cursors.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
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

	public sealed class Cursors{

		public static Cursor AppStarting {
			get { return new Cursor ( CursorType.IDC_APPSTARTING ); }
		}
		
		public static Cursor Arrow {
			get { return new Cursor ( CursorType.IDC_ARROW ); }
		}
		
		public static Cursor Cross {
			get { return new Cursor ( CursorType.IDC_CROSS ); }
		}
		
		[MonoTODO]
		public static Cursor Default {
			get { return new Cursor ( CursorType.IDC_ARROW ); }
		}
		
		public static Cursor Hand {
			get { return new Cursor ( CursorType.IDC_HAND ); }
		}
		
		public static Cursor Help {
			get { return new Cursor ( CursorType.IDC_HELP ); }
		}
		
		[MonoTODO]
		public static Cursor HSplit {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public static Cursor IBeam {
			get { return new Cursor ( CursorType.IDC_IBEAM ); }
		}
		
		public static Cursor No {
			get { return new Cursor ( CursorType.IDC_NO ); }
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
		
		public static Cursor SizeAll {
			get { return new Cursor ( CursorType.IDC_SIZEALL ); }
		}
		
		public static Cursor SizeNESW {
			get { return new Cursor ( CursorType.IDC_SIZENESW ); }
		}
		
		public static Cursor SizeNS {
			get { return new Cursor ( CursorType.IDC_SIZENS ); }
		}
		
		public static Cursor SizeNWSE {
			get { return new Cursor ( CursorType.IDC_SIZENWSE ); }
		}
		
		public static Cursor SizeWE {
			get { return new Cursor ( CursorType.IDC_SIZEWE ); }
		}
		
		public static Cursor UpArrow {
			get { return new Cursor ( CursorType.IDC_UPARROW ); }
		}
		
		[MonoTODO]
		public static Cursor VSplit {
			get {
				throw new NotImplementedException (); 
			}
		}
		
		public static Cursor WaitCursor {
			get { return new Cursor ( CursorType.IDC_WAIT ); }
		}
	}
}
