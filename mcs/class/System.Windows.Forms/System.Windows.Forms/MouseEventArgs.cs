//
// System.Windows.Forms.MouseEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class MouseEventArgs : EventArgs {
		private MouseButtons button;
		private int clicks;
		private int x;
		private int y;
		private int delta;

		public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
		{
			this.button = button;
			this.clicks = clicks;
			this.x = x;
			this.y = y;
			this.delta = delta;
		}

		//
		//  --- Public Properties
		//
		public MouseButtons Button {
			get {
				return button;
			}
		}
		public int Clicks {
			get {
				return clicks;
			}
		}
		public int Delta {
			get {
				return delta;
			}
		}
		public int X {
			get {
				return x;
			}
		}
		public int Y {
			get {
				return y;
			}
		}

//		//
//		//  --- Public Methods
//		//
//		[MonoTODO]
//		public virtual bool Equals(object)
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public static bool Equals(object, object)
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public virtual int GetHashCode()
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public Type GetType()
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public virtual string ToString()
//		{
//			throw new NotImplementedException ();
//		}
//
//		//
//		//  --- Protected Methods
//		//
//		[MonoTODO]
//		protected object MemberwiseClone()
//		{
//			throw new NotImplementedException ();
//		}
//
//		//
//		//  --- Destructor
//		//
//		[MonoTODO]
//		~MouseEventArgs()
//		{
//			throw new NotImplementedException ();
//		}
	 }
}
