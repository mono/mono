// System.BadImageFormatException
//
// Sean MacIsaac (macisaac@ximian.com)
// Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.

using System.Globalization;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class BadImageFormatException : SystemException
	{
		// Fields
		private string msg; // we need this because System.Exception's message is private.
		private Exception inner;
		private string fileName;
		private string fusionLog;
		
		// Constructors
		public BadImageFormatException ()
			: base (Locale.GetText ("Invalid file image."))
		{
			msg = "Invalid file image.";
		}
		
		public BadImageFormatException (string message)
			: base (message)
		{
			msg = message;
		}

		protected BadImageFormatException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			fileName = info.GetString ("BadImageFormat_FileName");
			fusionLog = info.GetString ("BadImageFormat_FusionLog");
		}

		public BadImageFormatException (string message, Exception inner)
			: base (message, inner)
		{
			msg = message;
			this.inner = inner;
		}

		public BadImageFormatException (string message, string fileName)
			: base (message)
		{
			msg = message;
			this.fileName = fileName;
		}

		public BadImageFormatException (string message, string fileName, Exception inner)
			: base (message, inner)
		{
			msg = message;
			this.inner = inner;
			this.fileName = fileName;
		}
		    
		// Properties
		public override string Message
		{
			get { return Locale.GetText (msg); }
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
			info.AddValue ("BadImageFormat_FileName", fileName);
			info.AddValue ("BadImageFormat_FusionLog", fusionLog);
		}

		public override string ToString ()
		{
			if (inner != null)
				return inner.ToString();
			else
				return base.ToString ();
		}
	}
}
