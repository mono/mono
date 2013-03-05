//
// System.IO.FileNotFoundException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO {

	[Serializable]
	[ComVisible (true)]
	public class FileNotFoundException : IOException {

		const int Result = unchecked ((int)0x80131621);

		private string fileName;
		private string fusionLog;

		// Constructors
		public FileNotFoundException ()
			: base (Locale.GetText ("Unable to find the specified file."))
		{
			HResult = Result;
		}

		public FileNotFoundException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public FileNotFoundException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		public FileNotFoundException (string message, string fileName)
			: base (message)
		{
			HResult = Result;
			this.fileName = fileName;
		}

		public FileNotFoundException (string message, string fileName, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
			this.fileName = fileName;
		}

		protected FileNotFoundException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			fileName = info.GetString ("FileNotFound_FileName");
			fusionLog = info.GetString ("FileNotFound_FusionLog");
		}

		public string FileName
		{
			get { return fileName; }
		}

		public string FusionLog {
			// note: MS runtime throws a SecurityException when the Exception is created
			// but a FileLoadException once the exception as been thrown. Mono always
			// throw a SecurityException in both case (anyway fusionLog is currently empty)
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return fusionLog; }
		}

		public override string Message {
			get {
				if (base.message == null) {
					if (fileName != null) {
						string message = string.Format (
							"Could not load file or assembly '{0}' or one of"
							+ " its dependencies. The system cannot find the"
							+ " file specified.", fileName);
						return message;
					}
				}
				return base.message;
			}
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("FileNotFound_FileName", fileName);
			info.AddValue ("FileNotFound_FusionLog", fusionLog);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder (GetType ().FullName);
			sb.AppendFormat (": {0}", Message);

			if (fileName != null && fileName.Length > 0) {
				sb.Append (Environment.NewLine);
				sb.AppendFormat ("File name: '{0}'", fileName);
			}

			if (this.InnerException != null)
				sb.AppendFormat (" ---> {0}", InnerException);

			if (this.StackTrace != null) {
				sb.Append (Environment.NewLine);
				sb.Append (StackTrace);
			}

			return sb.ToString ();
		}
	}
}
