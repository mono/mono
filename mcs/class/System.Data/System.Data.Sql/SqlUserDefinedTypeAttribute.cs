//
// System.Data.Sql.SqlUserDefinedTypeAttribute
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;

namespace System.Data.Sql {
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public sealed class SqlUserDefinedTypeAttribute : Attribute
	{
		#region Fields

		public const int MaxByteSizeValue = 8000;

		Format format;
		bool isByteOrdered;
		bool isFixedLength;
		int maxByteSize;

		#endregion // Fields

		#region Constructors

		public SqlUserDefinedTypeAttribute (Format f)
		{
			Format = f;
			IsByteOrdered = false;
			IsFixedLength = false;
			MaxByteSize = MaxByteSizeValue;
		}

		#endregion // Constructors

		#region Properties

		public Format Format { 
			get { return format; }
			set { format = value; }
		}

		public bool IsByteOrdered {
			get { return isByteOrdered; }
			set { isByteOrdered = value; }
		}

		public bool IsFixedLength {
			get { return isFixedLength; }
			set { isFixedLength = value; }
		}

		public int MaxByteSize {
			get { return maxByteSize; }
			set { maxByteSize = value; }
		}

		#endregion // Properties
	}
}

#endif
