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
	internal class TdsPacketRowResult : TdsPacketResult, IEnumerable
	{
		#region Fields

		TdsContext context;
		ArrayList list;

		#endregion // Fields

		#region Constructors

		public TdsPacketRowResult (TdsContext context)
			: base (TdsPacketSubType.Row)
		{
			this.context = context;
			list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public TdsContext Context {
			get { return context; }
		}

		public object this[int index] {
			get { 
				if (index > list.Count)
					throw new IndexOutOfRangeException ();
				return list[index]; 
			}
			set { list[index] = value; }
		}

		#endregion // Properties

		#region Methods 

		public int Add (object value)
		{
			return list.Add (value);
		}

		public void CopyTo (int index, Array array, int arrayIndex, int count)
		{
			list.CopyTo (index, array, arrayIndex, count);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion // Methods

	}
}
