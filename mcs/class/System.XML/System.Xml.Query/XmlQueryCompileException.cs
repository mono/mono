//
// System.Xml.Query.XmlQueryCompileException
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Runtime.Serialization;

namespace System.Xml.Query {
        public class XmlQueryCompileException : XmlQueryException
        {
		#region Constructors

		public XmlQueryCompileException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public XmlQueryCompileException ()
			: base ("An XML Query Compile Exception has occurred.")
		{
		}

		public XmlQueryCompileException (string res)
			: base (res)
		{
		}

		public XmlQueryCompileException (string resource, Exception exception)
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
