//
// System.Data.OleDb.OleDbPermissionAttribute
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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


using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
			| AttributeTargets.Struct | AttributeTargets.Constructor |
			AttributeTargets.Method)]
	[Serializable]
	public sealed class OleDbPermissionAttribute : DBDataPermissionAttribute
	{

		#region Constructors 

		[MonoTODO]
		OleDbPermissionAttribute (SecurityAction action) 
			: base (action)
		{
		}

		#endregion

		#region Properties

		[MonoTODO]
		public string Provider {
			[MonoTODO]
			get {
				throw new NotImplementedException (); 
			}
			[MonoTODO]
			set {
				throw new NotImplementedException (); 
			}
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override IPermission CreatePermission () 
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
