//
// System.BadImageFormatException.cs
//
// Authors:
//   Sean MacIsaac (macisaac@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[MonoTODO ("probably not entirely correct. fusionLog needs to be set somehow (we are probably missing internal constuctor)")]
	public class BadImageFormatException : SystemException
	{
		const int Result = unchecked ((int)0x8007000B);

		// Fields
		private string fileName;
		private string fusionLog;

		// Constructors
		public BadImageFormatException ()
			: base (Locale.GetText ("Invalid file image."))
		{
			HResult = Result;
		}

		public BadImageFormatException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected BadImageFormatException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			fileName = info.GetString ("BadImageFormat_FileName");
			fusionLog = info.GetString ("BadImageFormat_FusionLog");
		}

		public BadImageFormatException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		public BadImageFormatException (string message, string fileName)
			: base (message)
		{
			this.fileName = fileName;
			HResult = Result;
		}

		public BadImageFormatException (string message, string fileName, Exception innerException)
			: base (message, innerException)
		{
			this.fileName = fileName;
			HResult = Result;
		}

		// Properties
		public override string Message
		{
			get { return base.Message; }
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
			if (fileName != null)
				return Locale.GetText ("Filename: ") + fileName;
			return base.ToString ();
		}
	}
}
