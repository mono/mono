//
// System.Data.SqlTypes.SqlChars
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes
{
	[MonoTODO]
	public sealed class SqlChars : INullable, IXmlSerializable
	{
		#region Fields

		bool notNull;

		#endregion

		#region Constructors

		[MonoTODO]
		public SqlChars (char[] buffer)
		{
			notNull = true;
		}

		[MonoTODO]
		public SqlChars (SqlStreamChars s)
		{
			notNull = true;
		}

		[MonoTODO]
		public SqlChars (SqlString value)
		{
			notNull = true;
		}

		[MonoTODO]
		public SqlChars (IntPtr ptrBuffer, long length)
		{
			notNull = true;
		}

		#endregion

		#region Properties

                public bool IsNull {
                        get { return !notNull; }
                }

		#endregion

		#region Methods

		[MonoTODO]
		XmlSchema IXmlSerializable.GetSchema ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void IXmlSerializable.WriteXml (XmlWriter writer) 
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

#endif
