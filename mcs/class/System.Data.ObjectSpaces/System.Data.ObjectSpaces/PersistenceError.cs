//
// System.Data.ObjectSpaces.PersistenceError
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003-2004
//

#if NET_1_2

namespace System.Data.ObjectSpaces {
	public class PersistenceError 
	{
		#region Fields

		object errorObject;
		PersistenceErrorType errorType;
		Exception innerException;

		#endregion // Fields

		#region Constructors

		internal PersistenceError (object errorObject, PersistenceErrorType errorType, Exception innerException)
			: base ()
		{
			this.errorObject = errorObject;
			this.errorType = errorType;
			this.innerException = innerException;
		}

		#endregion // Constructors

		#region Properties

		public object ErrorObject {
			get { return errorObject; }
		}

		public PersistenceErrorType ErrorType {
			get { return errorType; }
		}

		public Exception InnerException {
			get { return innerException; }
				
		}
			
		#endregion // Properties
	}
}

#endif
