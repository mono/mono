//
// System.Xml.Query.XmlQueryCompileException
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Novell Inc., 2004
//

#if NET_2_0

using System;
using System.Runtime.Serialization;

namespace System.Xml.Query
{
	public class XmlQueryCompileException : XmlQueryException
	{
		#region Constructors

		protected XmlQueryCompileException (SerializationInfo info, StreamingContext context)
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

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
