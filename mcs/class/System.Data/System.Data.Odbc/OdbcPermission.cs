//
// System.Data.Odbc.OdbcPermission
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Novell Inc, 2004
//

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

using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Odbc
{
	[Serializable]
	public sealed class OdbcPermission : DBDataPermission
	{
		#region Constructors

		[MonoTODO]
#if NET_1_1
               [Obsolete ("use OdbcPermission(PermissionState.None)", true)]
#endif
		public OdbcPermission () : base (PermissionState.None)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OdbcPermission (PermissionState state)
			: base (state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
#if NET_1_1
		[Obsolete ("use OdbcPermission(PermissionState.None)", true)]
#endif
		public OdbcPermission (PermissionState state, bool allowBlankPassword)
			: base (state, allowBlankPassword, true)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		internal string Provider {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override IPermission Copy ()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public override void Add (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			throw new NotImplementedException ();
		}
		

		#endregion
	}
}
