//
// Mono.Data.SybaseClient.SybasePermission.cs
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
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace Mono.Data.SybaseClient {
	public sealed class SybasePermission : DBDataPermission 
	{
#if NET_2_0
		[Obsolete ("Use SybasePermission(PermissionState.None)", true)]
#endif
		[MonoTODO]
		public SybasePermission () 
#if NET_2_0
			: base (PermissionState.None)
#endif
		{
			// FIXME: do constructor
		}

		[MonoTODO]
		public SybasePermission (PermissionState state) 
			: base (state)
		{
			// FIXME: do constructor
		}

#if NET_2_0
		[Obsolete ("Use SybasePermission(PermissionState.None)", true)]
#endif
		[MonoTODO]
		public SybasePermission (PermissionState state, bool allowBlankPassword) 
#if NET_2_0
			: base (PermissionState.None)
#endif
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Copy () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target)  
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString () 
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
	}
}
