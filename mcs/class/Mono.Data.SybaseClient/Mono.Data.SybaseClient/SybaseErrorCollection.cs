//
// Mono.Data.SybaseClient.SybaseErrorCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//
using System;
using System.Collections;
using System.Data;
using System.Runtime.InteropServices;

namespace Mono.Data.SybaseClient {
	[MonoTODO]
	public sealed class SybaseErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList();

		#endregion // Fields

		#region Constructors

		internal SybaseErrorCollection () {
		}

		internal SybaseErrorCollection (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			Add (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties
                
		public int Count {
			get { return list.Count; }			  
		}

		bool ICollection.IsSynchronized {
			get { throw new NotImplementedException (); }			  
		}

		object ICollection.SyncRoot {
			get { throw new NotImplementedException (); }			  
		}

		public SybaseError this[int index] 
		{
			get { return (SybaseError) list[index]; }
		}

		#endregion // Properties

		#region Methods
		
		internal void Add(SybaseError error) 
		{
			list.Add(error);
		}

		internal void Add(byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			SybaseError error = new SybaseError(theClass, lineNumber, message, number, procedure, server, source, state);
			Add(error);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
