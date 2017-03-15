// 
// System.EnterpriseServices.RegistrationException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.Serialization;

namespace System.EnterpriseServices {
	[Serializable]
	public sealed class RegistrationException : SystemException {

		#region Fields
#pragma warning disable 649
		RegistrationErrorInfo[] errorInfo;
#pragma warning restore 649

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public RegistrationException (string msg)
			: base (msg)
		{
		}

		public RegistrationException ()
			: this ("Registration error")
		{
		}

		public RegistrationException (string msg, Exception inner)
			: base (msg, inner)
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
