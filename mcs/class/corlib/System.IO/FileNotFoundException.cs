//
// System.IO.FileNotFoundException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Runtime.Serialization;

namespace System.IO {

	[Serializable]
	public class FileNotFoundException : IOException {
		private string fileName;
		private string fusionLog;

		// Constructors
		public FileNotFoundException ()
			: base (Locale.GetText ("File not found"))
		{
		}

		public FileNotFoundException (string message)
			: base (message)
		{
		}

		public FileNotFoundException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public FileNotFoundException (string message, string fileName)
			: base (message)
		{
			this.fileName = fileName;
		}

		public FileNotFoundException (string message, string fileName, Exception innerException)
			: base (message, innerException)
		{
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

		public string FusionLog
		{
			get { return fusionLog; }
		}

		public override string Message
		{
			get {
				if (base.Message == null)
					return "File not found";

				if (fileName == null)
					return base.Message;
				
				return "File '" + fileName + "' not found.";
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
			string result = GetType ().FullName + ": " + Message;
			if (InnerException != null)
				result += " ----> " + InnerException.ToString ();

			if (StackTrace != null)
				result += "\n" + StackTrace;

			return result;
		}
	}
}
