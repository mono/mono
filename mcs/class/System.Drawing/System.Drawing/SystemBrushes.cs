//
// System.Drawing.SystemBrushes.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for SystemBrushes.
	/// </summary>
	public class SystemBrushes
	{
		public SystemBrushes()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static Brush Control {
			get {
				throw new NotImplementedException();
			}
		}

		public static Brush ControlText {
			get {
				return new SolidBrush(Color.Black);
			}
		}

		public static Brush Highlight {
			get {
				return new SolidBrush(Color.Blue);
			}
		}

		public static Brush HighlightText {
			get {
				return new SolidBrush(Color.White);
			}
		}

		public static Brush Window {
			get {
				return new SolidBrush(Color.White);
			}
		}
	}
}
