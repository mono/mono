// 
// System.EnterpriseServices.TransactionAttribute.cs
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
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	[ComVisible(false)]
	public sealed class TransactionAttribute : Attribute {

		#region Fields

		TransactionIsolationLevel isolation;
		int timeout;
		TransactionOption val;

		#endregion // Fields

		#region Constructors

		public TransactionAttribute ()
			: this (TransactionOption.Required)
		{
		}

		public TransactionAttribute (TransactionOption val)
		{
			this.isolation = TransactionIsolationLevel.Serializable;
			this.timeout = -1;
			this.val = val;
		}

		#endregion // Constructors

		#region Properties

		public TransactionIsolationLevel Isolation {
			get { return isolation; }
			set { isolation = value; }
		}

		public int Timeout {
			get { return timeout; }
			set { timeout = value; }
		}

		public TransactionOption Value {
			get { return val; }
		}

		#endregion // Properties
	}
}
