//
// System.Data.ObjectSpaces.PersistenceException
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Runtime.Serialization;

namespace System.Data.ObjectSpaces {
	public class PersistenceException : ObjectException
	{
		#region Fields

		ArrayList errors;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public PersistenceException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.errors = new ArrayList ();
		}

		[MonoTODO]
		public PersistenceException ()
			: base ("A PersistenceException has occurred.")
		{
			this.errors = new ArrayList ();
		}

		[MonoTODO]
		public PersistenceException (PersistenceError[] errors, string s)
			: base (s)
		{
			this.errors = new ArrayList (errors);
		}

		[MonoTODO]
		public PersistenceException (PersistenceError[] errors, string s, Exception innerException)
			: base (s, innerException)
		{
			this.errors = new ArrayList (errors);
		}

		#endregion // Constructors

		#region Properties

		public ArrayList Errors {
			get { return errors; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		#endregion Methods
	}
}

#endif
