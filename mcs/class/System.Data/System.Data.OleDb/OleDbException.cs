//
// System.Data.OleDb.OleDbException
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Data.OleDb
{
	[Serializable]
	public sealed class OleDbException : ExternalException
	{
		#region Properties

		public override int ErrorCode {	
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public OleDbErrorCollection Errors {	
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string Message {	
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string Source {	
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
