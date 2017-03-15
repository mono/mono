//
// System.Data.SqlClient.SqlCredential.cs
//
// Author:
//   Neale Ferguson (neale@sinenomine.net)
//
// Copyright (C) Neale Ferguson, 2014
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
using System.Data;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Data.SqlClient {
	/// <summary>
	/// Describes an error from a SQL database.
	/// </summary>
	[Serializable]
	public sealed class SqlCredential
	{
		#region Fields

		string uid = "";
		SecureString pwd = null;

		#endregion // Fields

		#region Constructors

		public SqlCredential (string userId, SecureString password)
		{
			if (userId == null)
				throw new ArgumentNullException("userId");
			if (password == null)
				throw new ArgumentNullException("password");
			this.uid = userId;
			this.pwd = password;
		}

		#endregion // Constructors
		
		#region Properties

		public string UserId {
			get { return uid; }
		}

		public SecureString Password {
			get { return pwd; }
		}

		#endregion
	}
}
