//
// System.Data.SqlXml.XmlRowsetAdapterException
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Runtime.Serialization;

namespace System.Data.SqlXml {
        public class XmlRowsetAdapterException : Exception
        {
		#region Constructors

		public XmlRowsetAdapterException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public XmlRowsetAdapterException ()
			: base ("An XML Rowset Adapter Exception has occurred.")
		{
		}

		public XmlRowsetAdapterException (string res)
			: base (res)
		{
		}

		public XmlRowsetAdapterException (string resource, Exception exception)
			: base (resource, exception)
		{
		}

		#endregion // Constructors

		#region Properties
	
		[MonoTODO]
		public override string Message {
			get { throw new NotImplementedException(); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
