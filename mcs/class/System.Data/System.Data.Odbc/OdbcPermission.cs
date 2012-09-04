//
// System.Data.Odbc.OdbcPermission
//
// Authors:
//	Umadevi S (sumadevi@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Odbc
{
	[Serializable]
	public sealed class OdbcPermission : DBDataPermission
	{
		#region Constructors

		[Obsolete ("use OdbcPermission(PermissionState.None)", true)]
		public OdbcPermission ()
			: base (PermissionState.None)
		{
		}

		public OdbcPermission (PermissionState state)
			: base (state)
		{
		}

		[Obsolete ("use OdbcPermission(PermissionState.None)", true)]
		public OdbcPermission (PermissionState state, bool allowBlankPassword)
			: base (state)
		{
			AllowBlankPassword = allowBlankPassword;
		}

		// required for Copy method
		internal OdbcPermission (DBDataPermission permission)
			: base (permission)
		{
		}

		// easier (and common) permission creation from attribute class
		internal OdbcPermission (DBDataPermissionAttribute attribute)
			: base (attribute)
		{
		}

		#endregion

		#region Methods

		public override IPermission Copy ()
		{
			return new OdbcPermission (this);
		}

		public override void Add (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			base.Add (connectionString, restrictions, behavior);
		}

		#endregion
	}
}
