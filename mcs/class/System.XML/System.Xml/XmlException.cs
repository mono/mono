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

		int lineNumber;
		int linePosition;

		#endregion

		#region Constructors

#if NET_1_0
#else
		public XmlException () 
			: base ()
		{
		}
#endif
		public XmlException (string message, Exception innerException) 
			: base (message, innerException)
		{
		}

		protected XmlException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.lineNumber = info.GetInt32 ("lineNumber");
			this.linePosition = info.GetInt32 ("linePosition");
		}

#if NET_1_0
		internal XmlException (string message)
#else
		public XmlException (string message)
#endif
			: base (message)
		{
		}

		internal XmlException (IXmlLineInfo li, string message) : base (message)
		{
			if (li != null) {
				this.lineNumber = li.LineNumber;
				this.linePosition = li.LinePosition;
			}
		}
		
#if NET_1_0
		internal XmlException (string message, int lineNumber, int linePosition)
#else
		public XmlException (string message, int lineNumber, int linePosition)
#endif
			: base (message)
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
			get {
				if (lineNumber == 0)
					return base.Message;

				return String.Format ("{0} Line {1}, position {2}.",
						      base.Message, lineNumber, linePosition);
			}
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
