//
// System.Data.OleDb.OleDbPermissionAttribute
//
// Authors:
//	Rodrigo Moya (rodrigo@ximian.com)
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Rodrigo Moya, 2002
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

using System.ComponentModel;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb {

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | 
			AttributeTargets.Struct | AttributeTargets.Constructor |
			AttributeTargets.Method, AllowMultiple=true,
			Inherited=false)]
	[Serializable]
	public sealed class OleDbPermissionAttribute : DBDataPermissionAttribute {

		#region Fields

		private string _provider;

		#endregion // Fields

		#region Constructors 

		public OleDbPermissionAttribute (SecurityAction action) 
			: base (action)
		{
		}

		#endregion

		#region Properties

#if NET_2_0
		[Obsolete ()]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
#endif
		public string Provider {
			get {
				if (_provider == null)
					return String.Empty;
				return _provider;
			}
			set { _provider = value; }
		}

		#endregion

		#region Methods

		public override IPermission CreatePermission () 
		{
			OleDbPermission p = new OleDbPermission (this);
#if !NET_2_0
			p.Provider = _provider;
#endif
			return p;
		}

		#endregion
	}
}
