//
// Mono.Data.TdsClient.Internal.TdsPacketRowResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketRowResult : TdsPacketResult
	{
		#region Fields

		TdsContext context;
		ArrayList row;

		#endregion // Fields

		#region Constructors

		public TdsPacketRowResult (TdsContext context)
			: base (TdsPacketSubType.Row)
		{
			this.context = context;
			row = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public TdsContext Context {
			get { return context; }
		}

		public object this[int index] {
			get { 
				if (index > row.Count)
					throw new IndexOutOfRangeException ();
				return row[index]; 
			}
			set { row[index] = value; }
		}

		#endregion // Properties

		#region Methods 

		public int Add (object value)
		{
			return row.Add (value);
		}

		#endregion // Methods

	}
}
