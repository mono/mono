//
// System.Data.Common.DbDataPermissionAttribute.cs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
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

using System;
using System.Security.Permissions;

namespace System.Data.Common {
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | 
			 AttributeTargets.Struct | AttributeTargets.Constructor | 
			 AttributeTargets.Method, AllowMultiple=true,
			 Inherited=false)]
	[Serializable]
	public abstract class DBDataPermissionAttribute : CodeAccessSecurityAttribute
	{
		#region Fields

		SecurityAction securityAction;
		bool allowBlankPassword;
#if NET_1_1
		KeyRestrictionBehavior keyRestrictionBehavior;
		String 	connectionString;
#endif

		#endregion // Fields

		#region Constructors

		protected DBDataPermissionAttribute (SecurityAction action) 
			: base (action) 
		{
			securityAction = action;
			allowBlankPassword = false;
		}

		#endregion // Constructors

		#region Properties

		public bool AllowBlankPassword {
			get { return allowBlankPassword; }
			set { allowBlankPassword = value; }
		}

		[MonoTODO]
		public string KeyRestrictions {
			get { 
				throw new NotImplementedException ();
			}	
			
			set {
				throw new NotImplementedException ();
			}
		}

#if NET_1_1
		public String ConnectionString {
			get { return connectionString; }
			set { connectionString = value; }
		}

		public KeyRestrictionBehavior KeyRestrictionBehavior {
			get { return keyRestrictionBehavior; }
			set { keyRestrictionBehavior = value; }
		}
#endif

		#endregion // Properties

		#region // Methods
#if NET_2_0
		[MonoTODO]
		public bool ShouldSerializeConnectionString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool ShouldSerializeKeyRestrictions ()
		{
			throw new NotImplementedException ();
		}
#endif
		#endregion // Methods
	}
}
