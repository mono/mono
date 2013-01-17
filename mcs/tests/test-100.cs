namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	public class Object  {
		int v;

		protected int Raw {
			get {
				return 1;
			}
			set {
				v = value;
			}
		}       

		[DllImport("bah", CallingConvention=CallingConvention.Cdecl)]
		static extern void g_object_get (int obj);

		public void GetProperty ()
		{
			g_object_get (Raw);
		}

		public static int Main ()
		{
			return 0;
		}
	}
}
