//
// System.Drawing.PaperSize.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
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
		PaperKind kind;
		
		public PaperSize(string name, int width, int height)
		{
			this.width = width;
			this.height = height;
			this.name = name;
			this.kind = PaperKind.Custom;
		}
		public int Width{
			get{
				return width;
			}set
			 {
			 	if (Kind != PaperKind.Custom)
			 		throw new ArgumentException();
				 width = value;
			 }
		}
		public int Height{
			get{
				return height;
			}set
			 {
			 	if (Kind != PaperKind.Custom)
			 		throw new ArgumentException();
				 height = value;
			 }
		}

		public string PaperName{
			get{
				return name;
			}
			set{
				if (Kind != PaperKind.Custom)
			 		throw new ArgumentException();
				 name = value;
			 }
		}
	
		public PaperKind Kind{
			get{
				return kind;
			}
		}

		public override string ToString(){
			string ret = "[PaperSize {0} Kind={1} Height={2} Width={3}]";
			return String.Format(ret, this.PaperName, this.Kind, this.Height, this.Width);
		}
	}
}
