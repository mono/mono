//
// System.Drawing.PaperSize.cs
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
	/// Summary description for PaperSize.
	/// </summary>
	public class PaperSize
	{
		string name;
		int width;
		int height;
		//Kind kind;
		public PaperSize(string name, int width, int height)
		{
			this.width = width;
			this.height = height;
			this.name = name;
		}
		public int Width{
			get{
				return width;
			}set
			 {
				 width = value;
			 }
		}
		public int Height{
			get{
				return height;
			}set
			 {
				 height = value;
			 }
		}

		public string PaperName{
			get{
				return name;
			}
			set{
				 name = value;
			 }
		}
	
//		public PaperKind Kind{
//			get{
//				return kind;
//			}
//		}

		[MonoTODO]
		public override string ToString(){
			return base.ToString();//FIXME: must override!
		}
	}
}
