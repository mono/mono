//
// System.Windows.Forms.PaintEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class PaintEventArgs : EventArgs, IDisposable {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public PaintEventArgs()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public Rectangle ClipRectangle {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Graphics Graphics {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public void Dispose()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}

		//public Type GetType()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected virtual void Dispose(bool disposing)
		{
			throw new NotImplementedException ();
		}
		//protected object MemberwiseClone()
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- Destructor
		//
		[MonoTODO]
		~PaintEventArgs()
		{
			throw new NotImplementedException ();
		}
	 }
}
