// 
// System.EnterpriseServices.RegistrationException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.Serialization;

namespace System.EnterpriseServices {
	[Serializable]
	public sealed class RegistrationException : SystemException {

		#region Fields

		RegistrationErrorInfo[] errorInfo;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public RegistrationException (string msg)
			: base (msg)
		{
		}

		#endregion // Constructors

		#region Properties
			
		public RegistrationErrorInfo[] ErrorInfo {
			get { return errorInfo; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext ctx)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
