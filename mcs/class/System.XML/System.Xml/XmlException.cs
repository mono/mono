//
// XmlException.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Runtime.Serialization;

namespace System.Xml
{
	[Serializable]
	public class XmlException : SystemException
	{
		#region Fields

		string msg; 	//  Cache message here because SystemException doesn't expose it
		int lineNumber;
		int linePosition;

		#endregion

		#region Constructors

		public XmlException (string message, Exception innerException) 
			: base (message, innerException)
		{
			msg = message;
		}

		protected XmlException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.lineNumber = info.GetInt32 ("lineNumber");
			this.linePosition = info.GetInt32 ("linePosition");
		}

		internal XmlException (string message)
			: base (message)
		{
			msg = message;
		}

		internal XmlException (string message, int lineNumber, int linePosition) : base (message)
		{
			this.lineNumber = lineNumber;
			this.linePosition = linePosition;
		}

		#endregion

		#region Properties

		public int LineNumber {
			get { return lineNumber; }
		}

		public int LinePosition {
			get { return linePosition; }
		}

		public override string Message {
			get { return msg; }
		}

		#endregion

		#region Methods

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("lineNumber", lineNumber);
			info.AddValue ("linePosition", linePosition);
		}

		#endregion
	}
}
