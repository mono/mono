// System.Xml.Xsl.XsltException
// 
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

using System;
using System.Runtime.Serialization;

namespace System.Xml.Xsl
{
	[Serializable]
	public class XsltException : SystemException
	{
		#region Fields

		string _message; 
		int _lineNumber;
		int _linePosition;
		string _sourceUri;

		#endregion

		#region Constructors

		public XsltException(
			string message,
			Exception innerException ) 
			: base (message, innerException)
		{
			_message = message;
		}

		protected XsltException(
			SerializationInfo info,
			StreamingContext context )
		{
			_lineNumber = info.GetInt32 ("lineNumber");
			_linePosition = info.GetInt32 ("linePosition");
			_sourceUri = info.GetString ("sourceUri");
		}
		
		#endregion

		#region Properties

		public int LineNumber {	
			get { return _lineNumber; }
		}

		public int LinePosition {
			get { return _linePosition; }
		}

		public override string Message {
			get { return _message; }
		}

		public string SourceUri {
			get { return _sourceUri; }
		}

		#endregion

		#region Methods

		public override void GetObjectData(
			SerializationInfo info,
			StreamingContext context )
		{
			base.GetObjectData (info, context);
			info.AddValue ("lineNumber", _lineNumber);
			info.AddValue ("linePosition", _linePosition);
			info.AddValue ("sourceUri", _sourceUri);
		}

		#endregion
	}
}
