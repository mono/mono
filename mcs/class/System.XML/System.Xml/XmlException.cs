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
	public class XmlException : SystemException
	{
		#region Fields

		int lineNumber;
		int linePosition;

		#endregion

		#region Constructors

		public XmlException (string message, Exception innerException) 
			: base (message, innerException)
		{
		}

		[MonoTODO]
		protected XmlException (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		internal XmlException (string message) : base (message)
		{
		}

		internal XmlException (string message, int lineNumber, int linePosition) : base (message)
		{
			this.lineNumber = lineNumber;
			this.linePosition = linePosition;
		}

		internal XmlException (string message, XmlInputSource inputSrc)
		{
		}

		#endregion

		#region Properties

		public int LineNumber 
		{
			get { return lineNumber; }
		}

		public int LinePosition 
		{
			get { return linePosition; }
		}

		#endregion
	}
}
