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
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class Screen {

		//
		//  --- Public Properties
		//
		
		internal static ArrayList allScreens = new ArrayList();
		
		private Rectangle bounds;
		
		static Screen() {
			allScreens.Add (new Screen());
		}
		
		internal Screen() {
			bounds = new Rectangle(0, 0, 1024, 768);
		}
		
		[MonoTODO]
		public static Screen[] AllScreens {
			get {
				Screen[] result = new Screen[allScreens.Count];
				allScreens.CopyTo (result, 0);
				return result;
			}
		}
		[MonoTODO]
		public Rectangle Bounds {
			get {
				return bounds;
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
				return bounds;
			}
		}

		
		//  --- Public Methods
		
		[MonoTODO]
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
		[MonoTODO]
		public static Screen FromControl(Control ctl)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Screen FromHandle(IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Screen FromPoint(Point point)
		{
			return allScreens[0] as Screen;
		}
		[MonoTODO]
		public static Screen FromRectangle(Rectangle rect)
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
