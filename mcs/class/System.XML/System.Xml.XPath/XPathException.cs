//
// System.Xml.XPath.XPathException
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Copyright 2002 Tim Coleman
//

using System.Runtime.Serialization;

namespace System.Xml.XPath
{
	[Serializable]
	public class XPathException : SystemException
	{
#region Constructors

		protected XPathException (SerializationInfo info, StreamingContext context) : base (info, context) {}

		public XPathException (string message, Exception innerException) : base (message, innerException) {}

		internal XPathException (string message) : base (message, null) {}

#endregion

#region Properties

		public override string Message {

			get { return base.Message; }

		}

#endregion

#region Methods

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}

#endregion
	}
}
