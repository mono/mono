//
// OracleString.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Data.SqlTypes;

namespace System.Data.OracleClient {
	public struct OracleString : IComparable, INullable
	{
		#region Fields

		string value;
		bool notNull;

		public static readonly OracleString Empty = new OracleString (String.Empty);
		public static readonly OracleString Null = new OracleString ();

		#endregion // Fields

		#region Constructors

		public OracleString (string s)
		{
			value = s;
			notNull = true;
		}

		#endregion // Constructors

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public string Value {
			get { return value; }
		}

		#endregion // Properties

		#region Methods

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is OracleString))
				throw new ArgumentException ("Value is not a System.Data.OracleClient.OracleString");
			else if (((OracleString) value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((OracleString) value).Value);
		}

		#endregion // Methods
	}
}
