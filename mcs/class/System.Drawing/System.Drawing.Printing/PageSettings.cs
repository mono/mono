//
// System.Drawing.PageSettings.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing.Printing
{
	/// <summary>
	/// Summary description for PageSettings.
	/// </summary>
	public class PageSettings {//: IClonable
		//[MonoTODO]
		public PageSettings()
		{
			throw new NotImplementedException ();
		}
//props
		//[MonoTODO]
//		public Rectangle Bounds{
//			get{
//				throw new NotImplementedException ();
//			}
//			set{
//				throw new NotImplementedException ();
//			}
//		}
		//[MonoTODO]
		public bool Color{
			get{
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}

		//[MonoTODO]
		public bool Landscape {
			get{
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		//[MonoTODO]
		public Margins Margins{
			get{
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		//[MonoTODO]
		public PaperSize PaperSize{
			get{
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		//[MonoTODO]
		public PaperSource PaperSource{
			get{
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		//[MonoTODO]
		public PrinterResolution PrinterResolution{
			get{
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		//[MonoTODO]
//		public PrinterSetting PrinterSetting{
//			get{
//				throw new NotImplementedException ();
//			}
//			set{
//				throw new NotImplementedException ();
//			}
//		}
		//[ComVisible(false)]
		//[MonoTODO]
		public object Clone(){
			throw new NotImplementedException ();
		}

		//[ComVisible(false)]
		//[MonoTODO]
		public void CopyToHdevmode(IntPtr hdevmode){
			throw new NotImplementedException ();
		}

		//[ComVisible(false)]
		//[MonoTODO]
		public void SetHdevmode(IntPtr hdevmode){
			throw new NotImplementedException ();
		}
	
		//[ComVisible(false)]
		//[MonoTODO]
		public override string ToString(){
			//FIXME:  //THIS is wrong! This method is to be overridden!
			return base.ToString();
		}

	}
}
