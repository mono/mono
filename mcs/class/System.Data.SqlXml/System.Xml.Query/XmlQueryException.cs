//
// System.Xml.Query.XmlQueryException
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Data.SqlXml;
using System.Runtime.Serialization;

namespace System.Xml.Query {
        public class XmlQueryException : SystemException
        {
		#region Constructors

		public XmlQueryException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public XmlQueryException ()
			: base ("An XML Query Exception has occurred.")
		{
		}

		public XmlQueryException (string res)
			: base (res)
		{
		}

		public XmlQueryException (string resource, Exception exception)
			: base (resource, exception)
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public int LineNumber {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int LinePosition {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public override string Message {
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public string SourceUri {
			get { throw new NotImplementedException(); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void SetMessage (string message, bool addLineInfo)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
