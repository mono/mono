//
// System.Windows.Forms.InputLanguage.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Globalization;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public sealed class InputLanguage {

		private InputLanguage(){//For signiture compatablity. Prevents the auto creation of public constructor
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public CultureInfo Culture {
			get {throw new NotImplementedException ();}
		}
		[MonoTODO]
		public static InputLanguage CurrentInputLanguage {
			get { throw new NotImplementedException ();	}
			set { throw new NotImplementedException ();	}
		}
		[MonoTODO]
		public static InputLanguage DefaultInputLanguage {
			get { throw new NotImplementedException ();	}
		}
		[MonoTODO]
		public IntPtr Handle {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static InputLanguageCollection InstalledInputLanguages {
			get { throw new NotImplementedException ();	}
		}
		[MonoTODO]
		public string LayoutName {
			get { throw new NotImplementedException ();	}
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override bool Equals(object value){
			//FIXME:
			return base.Equals(value);
		}

		[MonoTODO]
		public static InputLanguage FromCulture(CultureInfo culture){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override int GetHashCode(){
			//FIXME:
			return base.GetHashCode();
		}
	 }
}
