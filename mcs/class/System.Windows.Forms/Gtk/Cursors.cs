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
	// It does nothing but returning a default cursor.
	// We need to implement Cursor before working on this class.

	public sealed class Cursors{

		private Cursors(){//for signtute compatablity
		}

		[MonoTODO]
		public static Cursor AppStarting {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor Arrow {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor Cross {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor Default {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor Hand {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor Help {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor HSplit {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor IBeam {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor No {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor NoMove2D {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor NoMoveHoriz {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor NoMoveVert {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor PanEast {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor PanNE {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor PanNorth {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor PanNW {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor PanSE {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor PanSouth {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor PanSW {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor PanWest {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor SizeAll {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor SizeNESW {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor SizeNS {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor SizeNWSE {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor SizeWE {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor UpArrow {
			get { return new Cursor(); }
		}
		
		[MonoTODO]
		public static Cursor VSplit {
			get { return new Cursor(); }
		}
		[MonoTODO]
		public static Cursor WaitCursor {
			get { return new Cursor(); }
		}
	}
}
