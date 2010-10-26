//
// System.IO.FileLoadException.cs
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
using System.Security;
#if !DISABLE_SECURITY
using System.Security.Permissions;
#endif
using System.Text;

#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.IO {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public class FileLoadException : IOException {

		// Fields
		const int Result = unchecked ((int)0x80070002);
		string msg;
		string fileName;
		string fusionLog;
		
		// Constructors
		public FileLoadException ()
			: base (Locale.GetText ("I/O Error"))
		{
			HResult = Result;
			msg = Locale.GetText ("I/O Error");
		}

		public FileLoadException (string message)
			: base (message)
		{
			HResult = Result;
			msg = message;
		}

		public FileLoadException (string message, string fileName)
			: base (message)
		{
			HResult = Result;
			this.msg = message;
			this.fileName = fileName;
		}		

		public FileLoadException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
			msg = message;
		}

		public FileLoadException (string message, string fileName, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
			this.msg = message;
			this.fileName = fileName;
		}

		protected FileLoadException (SerializationInfo info, StreamingContext context)
		{
			fileName = info.GetString ("FileLoad_FileName");
			fusionLog = info.GetString ("FileLoad_FusionLog");
		}

		// Properties
		public override string Message {
			get { return msg; }
		}

		public string FileName
		{
			get { return fileName; }
		}
		
		public string FusionLog	{
			// note: MS runtime throws a SecurityException when the Exception is created
			// but a FileLoadException once the exception as been thrown. Mono always
			// throw a SecurityException in both case (anyway fusionLog is currently empty)
			#if !DISABLE_SECURITY
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			#endif
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
			StringBuilder sb = new StringBuilder (GetType ().FullName);
			sb.AppendFormat (": {0}", msg);

			if (fileName != null)
				sb.AppendFormat (" : {0}", fileName);

			if (this.InnerException != null)
				sb.AppendFormat (" ----> {0}", InnerException);

			if (this.StackTrace != null) {
				sb.Append (Environment.NewLine);
				sb.Append (StackTrace);
			}

			return sb.ToString ();
		}
	}
}
