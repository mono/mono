//
// System.Data.SqlTypes.SqlXmlReader
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

namespace System.Data.SqlTypes
{
	[MonoTODO]
	public struct SqlXmlReader : INullable, IComparable
	{
		#region Fields

		public static readonly SqlXmlReader Null;

		bool notNull;

		#endregion

		#region Constructors

		[MonoTODO]
		public SqlXmlReader (XmlReader value)
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
			else if (!(value is SqlXmlReader))   
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlXmlReader"));
			else if (((SqlXmlReader)value).IsNull)
				return 1;
			else
				throw new NotImplementedException ();
		}

		#endregion
	}
}

#endif
