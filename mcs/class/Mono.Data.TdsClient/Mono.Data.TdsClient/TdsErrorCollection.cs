//
// Mono.Data.TdsClient.TdsErrorCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.Tds.Protocol;
using System;
using System.Collections;

namespace Mono.Data.TdsClient {
        public class TdsErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors

		internal TdsErrorCollection ()
		{
		}

		internal TdsErrorCollection (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
		{
			Add (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public TdsError this [int index] {
			get { return (TdsError) list[index]; }
		}

		bool ICollection.IsSynchronized {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		object ICollection.SyncRoot {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		internal void Add (TdsError error)
		{
			list.Add (error);
		}

		internal void Add (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
		{
			Add (new TdsError (theClass, lineNumber, message, number, procedure, server, source, state));
		}

		[MonoTODO]
		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion // Methods
	}
}
