//
// System.Windows.Forms.InputLanguageChangeEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Globalization;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class InputLanguageChangeEventArgs : EventArgs {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public InputLanguageChangeEventArgs ( CultureInfo culture, byte b)
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public byte CharSet {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public CultureInfo Culture {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public InputLanguage InputLanguage {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		//[MonoTODO]
		//public virtual bool Equals(object o);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public static bool Equals(object o1, object o2);
		//{
		//	throw new NotImplementedException ();
		//}
	 }
}
