// 
// System.EnterpriseServices.ServicedComponentException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[Serializable]
	public sealed class ServicedComponentException : SystemException {

		#region Constructors

		public ServicedComponentException ()
			: base ()
		{
		}

		public ServicedComponentException (string message)
			: base (message)
		{
		}

		public ServicedComponentException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		#endregion // Constructors
	}
}
