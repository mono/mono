//
// System.Drawing.InvalidPrinterExecption.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;
using System.Runtime.Serialization;

namespace System.Drawing.Printing
{
	/// <summary>
	/// Summary description for InvalidPrinterExecption.
	/// </summary>
	public class InvalidPrinterException : SystemException {
		private PrinterSettings settings;

		public InvalidPrinterException(PrinterSettings settings) {
			this.settings = settings;
		}
		protected InvalidPrinterException(SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}
	}
}
