////
//// System.Windows.Forms.AmbientProperties
////
//// Author:
////   Jaak Simm (jaaksimm@firm.ee)
////
//// (C) Ximian, Inc 2002
////
//
//
//using System;
//using System.Drawing;
//
//namespace System.Windows.Forms
//{
//	/// <summary>
//	/// Provides ambient property values to top-level controls.
//	/// </summary>
//	
//	public sealed class AmbientProperties
//	{
//		Color backcolor;
//		Cursor cursor;
//		Font font;
//		Color forecolor;
//		
//		
//		// --- (public) Properties ---
//		public Color BackColor {
//			get { return backcolor; }
//			set { backcolor = value; }
//		}
//	
//		public Cursor Cursor {
//			get { return cursor; }
//			set { cursor = value; }
//		}
//	
//		public Font Font {
//			get { return font; }
//			set { font = value; }
//		}
//	
//		public Color Forecolor {
//			get { return forecolor; }
//			set { forecolor = value; }
//		}
//		
//		
//		
//		// --- Constructor ---
//		public AmbientProperties()
//		{
//			// default values:
//			this.backcolor=Color.Empty;
//			this.cursor=null;
//			this.font=null;
//			this.forecolor=Color.Empty;
//		}
//	}
//}
