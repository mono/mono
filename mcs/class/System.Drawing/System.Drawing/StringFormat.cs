//
// System.Drawing.StringFormat.cs
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
	/// Summary description for StringFormat.
	/// </summary>
	public class StringFormat
	{
		private StringAlignment alignment;
		private StringAlignment line_alignment;

		public StringFormat()
		{
			//
			// TODO: Add constructor logic here
			//
			alignment = StringAlignment.Center;
			line_alignment = StringAlignment.Center;
		}
		
		public StringAlignment Alignment {
			get {
				return alignment;
			}

			set {
				alignment = value;
			}
		}

		public StringAlignment LineAlignment
		{
			get {
				return line_alignment;
			}

			set {
				line_alignment = value;
			}
		}
	}
}
