//
// System.Data.SqlTypes.SqlUtcDateTime
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
	public struct SqlUtcDateTime : INullable, IComparable, IXmlSerializable
	{
		#region Fields

		public static readonly SqlUtcDateTime MaxValue;
		public static readonly SqlUtcDateTime MinValue;

		bool notNull;

		#endregion

		#region Constructors

		[MonoTODO]
		public SqlUtcDateTime (long tickCount)
		{
			notNull = true;
		}

		[MonoTODO]
		public SqlUtcDateTime (int years, int months, int days)
		{
			notNull = true;
		}

		[MonoTODO]
		public SqlUtcDateTime (int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
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
			else if (!(value is SqlUtcDateTime))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlUtcDateTime"));
			else if (((SqlUtcDateTime)value).IsNull)
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
