//
// Mono.Data.TdsClient.Internal.TdsPacketErrorResultCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Data;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketErrorResultCollection : IEnumerable
	{
		#region Fields

		ArrayList list;
		
		#endregion // Fields

		#region Constructors

		public TdsPacketErrorResultCollection ()
		{
			list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public TdsPacketErrorResult this [int index] {
			get { return (TdsPacketErrorResult) list[index]; }
			set { list[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (TdsPacketErrorResult error)
		{
			return (list.Add (error));
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion // Methods
	}
}
