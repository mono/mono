//
// Mono.Data.Tds.Protocol.TdsPacketTableNameResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace Mono.Data.Tds.Protocol {
	public class TdsPacketTableNameResult : TdsPacketResult, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors

		public TdsPacketTableNameResult ()
			: base (TdsPacketSubType.TableName)
		{
		}

		#endregion // Constructors

		#region Properties

		public string this [int index] {
			get { return (string) list [index]; }
			set { list [index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (string tableName)
		{
			return list.Add (tableName);	
		}	

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion // Methods
	}
}
