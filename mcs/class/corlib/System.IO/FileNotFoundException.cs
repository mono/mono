//
// System.IO.FileNotFoundException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
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
