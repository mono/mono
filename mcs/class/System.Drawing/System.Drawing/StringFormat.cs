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
		protected StringAlignment Alignment_;
		protected StringAlignment LineAlignment_;

		public StringFormat()
		{
			//
			// TODO: Add constructor logic here
			//
			Alignment_ = StringAlignment.Center;
			LineAlignment_ = StringAlignment.Center;
		}
		
		public StringAlignment Alignment {
			get {
				return Alignment_;
			}

			set {
				Alignment_ = value;
			}
		}

		public StringAlignment LineAlignment
		{
			get {
				return LineAlignment_;
			}

			set {
				LineAlignment_ = value;
			}
		}
	}
}
