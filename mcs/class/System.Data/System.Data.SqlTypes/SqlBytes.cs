//
// System.Data.SqlTypes.SqlBytes
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes
{
	[MonoTODO]
	public sealed class SqlBytes : INullable, IXmlSerializable
	{
		#region Fields

		bool notNull;

		#endregion

		#region Constructors

		[MonoTODO]
		public SqlBytes (byte[] buffer)
		{
			notNull = true;
		}

		[MonoTODO]
		public SqlBytes (SqlBinary value)
			: this (value.Value)
		{
		}

		[MonoTODO]
		public SqlBytes (Stream s)
		{
			notNull = true;
		}

		[MonoTODO]
		public SqlBytes (IntPtr buffer, long length)
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
