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
		#region Fields
		
		string message;

		#endregion
		
		#region Constructors

		[MonoTODO]
		protected XPathException (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathException (string message, Exception innerException)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties
		
		public override string Message {
			get { return message; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		#endregion
	}
}
