// 
// System.IO.ErrorEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.IO {
	public class ErrorEventArgs : EventArgs {

		#region Fields

		Exception exception;

		#endregion // Fields

		#region Constructors

		public ErrorEventArgs (Exception exception) 
		{
			this.exception = exception;
		}
		
		#endregion // Constructors

		#region Methods

		public virtual Exception GetException ()
		{
			return exception;
		}

		#endregion // Methods
	}
}
