//
// System.IO.FileLoadException.cs
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
	public class FileLoadException : IOException {

		// Fields
		string msg;
		Exception inner;
		string fileName;
		string fusionLog;
		
		// Constructors
		public FileLoadException ()
			: base (Locale.GetText ("I/O Error"))
		{
			msg = Locale.GetText ("I/O Error");
		}

		public FileLoadException (string message)
			: base (message)
		{
			msg = message;
		}

		public FileLoadException (string message, string fileName)
			: base (message)
		{
			this.msg = message;
			this.fileName = fileName;
		}		

		public FileLoadException (string message, Exception inner)
			: base (message, inner)
		{
			msg = message;
			this.inner = inner;
		}

		public FileLoadException (string message, string fileName, Exception inner)
			: base (message, inner)
		{
			this.msg = message;
			this.fileName = fileName;
			this.inner = inner;
		}

		protected FileLoadException (SerializationInfo info, StreamingContext context)
		{
			fileName = info.GetString ("FileLoad_FileName");
			fusionLog = info.GetString ("FileLoad_FusionLog");
		}

		// Properties
		public override string Message
		{
			get {
				if (fileName != null)
					return Locale.GetText (msg + ": " + fileName);
				else
					return msg;
			}
		}

		public string FileName
		{
			get { return fileName; }
		}
		
		public string FusionLog
		{
			get { return fusionLog; }
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("FileLoad_FileName", fileName);
			info.AddValue ("FileLoad_FusionLog", fusionLog);
		}

		public override string ToString ()
		{
			string result = GetType ().FullName + ": " + Message;
			if (this.InnerException != null)
				result +=" ----> " + InnerException;
			if (this.StackTrace != null)
				result += '\n' + StackTrace;

			return result;
		}
	}
}
