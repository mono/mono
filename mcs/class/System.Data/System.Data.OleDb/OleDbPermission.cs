//
// System.Data.OleDb.OleDbPermission
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
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

using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb {

	[Serializable]
	public sealed class OleDbPermission : DBDataPermission {

		#region Constructors

#if NET_1_1
                [Obsolete ("use OleDbPermission(PermissionState.None)", true)]
#endif
		public OleDbPermission ()
			: base (PermissionState.None)
		{
		}

		public OleDbPermission (PermissionState state)
			: base (state)
		{
		}

#if NET_1_1
                [Obsolete ("use OleDbPermission(PermissionState.None)", true)]
#endif
		public OleDbPermission (PermissionState state, bool allowBlankPassword)
			: base (state)
		{
			AllowBlankPassword = allowBlankPassword;
		}

		// required for Copy method
		internal OleDbPermission (DBDataPermission permission)
			: base (permission)
		{
		}

		#endregion

		#region Properties
		#endregion

		#region Methods

		public override IPermission Copy ()
		{
			return new OleDbPermission (this);
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement)
		{
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SecurityElement ToXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
