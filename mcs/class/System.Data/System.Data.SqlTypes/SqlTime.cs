//
// System.Data.SqlTypes.SqlTime
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
	public struct SqlTime : INullable, IComparable, IXmlSerializable
	{
		#region Fields

		public static readonly SqlTime MaxValue;
		public static readonly SqlTime MinValue;

		bool notNull;

		#endregion

		#region Constructors

		[MonoTODO]
		public SqlTime (long tickCount)
		{
			notNull = true;
		}

		[MonoTODO]
		public SqlTime (int hours, int minutes, int seconds, int milliseconds)
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

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlTime))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlTime"));
			else if (((SqlTime)value).IsNull)
				return 1;
			else
				throw new NotImplementedException ();
		}

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
