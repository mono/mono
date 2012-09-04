//
// System.Data.Common.DbDataPermissionAttribute.cs
//
// Authors:
//	Rodrigo Moya (rodrigo@ximian.com)
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Ximian, Inc
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
using System.Security.Permissions;
using System.Globalization;

namespace System.Data.Common {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | 
			 AttributeTargets.Struct | AttributeTargets.Constructor | 
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public abstract class DBDataPermissionAttribute : CodeAccessSecurityAttribute {
		#region Fields

		bool allowBlankPassword;
		string keyRestrictions;
		KeyRestrictionBehavior keyRestrictionBehavior;
		string connectionString;

		#endregion // Fields

		#region Constructors

		protected DBDataPermissionAttribute (SecurityAction action) 
			: base (action) 
		{
		}

		#endregion // Constructors

		#region Properties

		public bool AllowBlankPassword {
			get { return allowBlankPassword; }
			set { allowBlankPassword = value; }
		}

		public string KeyRestrictions {
			get {
				if (keyRestrictions == null)
					return String.Empty;
				return keyRestrictions;
			}
			set { keyRestrictions = value; }
		}

		public string ConnectionString {
			get {
				if (connectionString == null)
					return String.Empty;
				return connectionString;
			}
			set { connectionString = value; }
		}

		public KeyRestrictionBehavior KeyRestrictionBehavior {
			get { return keyRestrictionBehavior; }
			set {
				ExceptionHelper.CheckEnumValue (typeof (KeyRestrictionBehavior), value);
				keyRestrictionBehavior = value;
			}
		}

		#endregion // Properties

		#region // Methods
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public bool ShouldSerializeConnectionString ()
		{
			// FIXME: configurable ? why is this in the attribute class ?
			return false;
		}

		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public bool ShouldSerializeKeyRestrictions ()
		{
			// FIXME: configurable ? why is this in the attribute class ?
			return false;
		}
		#endregion // Methods
	}
}
