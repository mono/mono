//
// System.Data.Sql.SqlUserDefinedAggregateAttribute
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
	public sealed class SqlUserDefinedAggregateAttribute : Attribute
	{
		#region Fields

		public const int MaxByteSizeValue = 8000;

		Format format;
		bool isInvariantToDuplicates;
		bool isInvariantToNulls;
		bool isInvariantToOrder;
		bool isNullIfEmpty;
		int maxByteSize;

		#endregion // Fields

		#region Constructors

		public SqlUserDefinedAggregateAttribute (Format f)
		{
			Format = f;
			IsInvariantToDuplicates = false;
			IsInvariantToNulls = false;
			IsInvariantToOrder = false;
			IsNullIfEmpty = false;
			MaxByteSize = MaxByteSizeValue;
		}

		#endregion // Constructors

		#region Properties

		public Format Format { 
			get { return format; }
			set { format = value; }
		}

		public bool IsInvariantToDuplicates {
			get { return isInvariantToDuplicates; }
			set { isInvariantToDuplicates = value; }
		}

		public bool IsInvariantToNulls {
			get { return isInvariantToNulls; }
			set { isInvariantToNulls = value; }
		}

		public bool IsInvariantToOrder {
			get { return isInvariantToOrder; }
			set { isInvariantToOrder = value; }
		}

		public bool IsNullIfEmpty {
			get { return isNullIfEmpty; }
			set { isNullIfEmpty = value; }
		}

		public int MaxByteSize {
			get { return maxByteSize; }
			set { maxByteSize = value; }
		}

		#endregion // Properties
	}
}

#endif
