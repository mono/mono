//
// System.Windows.Forms.Screen.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Runtime.InteropServices;
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class Screen {

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public static Screen[] AllScreens {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Rectangle Bounds {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string DeviceName {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Primary {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public static Screen PrimaryScreen {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Rectangle WorkingArea {
			get {
				throw new NotImplementedException ();
			}
		}

		
		//  --- Public Methods
		
		[MonoTODO]
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
		[MonoTODO]
		public static Screen FormControl(Control ctl)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Screen FormHandle(IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Screen FormPoint(Point point)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Screen FormRectangle(Rectangle rect)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Rectangle GetBounds(Control ctl)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Rectangle GetBounds(Point pt)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Rectangle GetBounds(Rectangle rect)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		[MonoTODO]
		public static Rectangle GetWorkingArea(Control ctl)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			return base.ToString();
		}
	 }
}
