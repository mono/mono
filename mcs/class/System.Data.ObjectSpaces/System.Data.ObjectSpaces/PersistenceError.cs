//
// System.Data.ObjectSpaces.PersistenceError
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.ObjectSpaces {
	public class PersistenceError 
	{
		#region Properties

		[MonoTODO]
		public object ErrorObject {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public PersistenceErrorType ErrorType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public Exception InnerException {
			get { throw new NotImplementedException (); }
		}
			
		#endregion // Properties
	}
}

#endif
