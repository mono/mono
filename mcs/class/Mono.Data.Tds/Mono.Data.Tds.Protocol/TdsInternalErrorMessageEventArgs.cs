//
// Mono.Data.TdsClient.Internal.TdsInternalErrorMessageEventArgs.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace Mono.Data.TdsClient.Internal {
	internal sealed class TdsInternalErrorMessageEventArgs : TdsInternalInfoMessageEventArgs 
	{
		#region Constructors
		
		public TdsInternalErrorMessageEventArgs (TdsInternalError error)
			: base (error)
		{
		}

		#endregion // Constructors
	}
}
