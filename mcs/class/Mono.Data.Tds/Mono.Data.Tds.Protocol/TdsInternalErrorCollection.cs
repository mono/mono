//
// Mono.Data.TdsClient.Internal.TdsInternalErrorCollection.cs
//
// Authors:
//    Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace Mono.Data.TdsClient.Internal {
	internal sealed class TdsInternalErrorCollection : IEnumerable 
	{
		#region Fields

		ArrayList list;

		#endregion // Fields

		#region Constructors

		public TdsInternalErrorCollection ()
		{
			list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public TdsInternalError this [int index] {
			get { return (TdsInternalError) list[index]; }
			set { list[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (TdsInternalError error)
		{
			return list.Add (error);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion // Methods
	}
}

