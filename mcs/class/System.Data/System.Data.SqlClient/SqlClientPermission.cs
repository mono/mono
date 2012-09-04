//
// System.Data.SqlClient.SqlClientPermission.cs
//
// Authors:
//	Rodrigo Moya (rodrigo@ximian.com)
//	Daniel Morgan (danmorg@sc.rr.com)
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
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

using System.Collections;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.SqlClient {

	[Serializable]
	public sealed class SqlClientPermission : DBDataPermission {

		#region Fields
		#endregion // Fields

		#region Constructors

		[Obsolete ("Use SqlClientPermission(PermissionState.None)", true)]
		public SqlClientPermission ()
			: this (PermissionState.None)
		{
		}

		public SqlClientPermission (PermissionState state) 
			: base (state)
		{
		}

		[Obsolete ("Use SqlClientPermission(PermissionState.None)", true)]
		public SqlClientPermission (PermissionState state, bool allowBlankPassword) 
			: base (state)
		{
			AllowBlankPassword = allowBlankPassword;
		}

		// required for Copy method
		internal SqlClientPermission (DBDataPermission permission)
			: base (permission)
		{
		}

		// easier (and common) permission creation from attribute class
		internal SqlClientPermission (DBDataPermissionAttribute attribute)
			: base (attribute)
		{
		}

		#endregion // Constructors

		#region Methods

		public override IPermission Copy ()
		{
			return new SqlClientPermission (this);
		}

		public override void Add (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			base.Add (connectionString, restrictions, behavior);
		}
		#endregion // Methods
	}
}
