// 
// System.EnterpriseServices.SecurityRoleAttribute.cs
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
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple=true)]
	[ComVisible(false)]
	public sealed class SecurityRoleAttribute : Attribute {

		#region Fields

		string description;
		bool everyone;
		string role;

		#endregion // Fields

		#region Constructors

		public SecurityRoleAttribute (string role)
			: this (role, false)
		{
		}

		public SecurityRoleAttribute (string role, bool everyone)
		{
			this.description = String.Empty;
			this.everyone = everyone;
			this.role = role;
		}

		#endregion // Constructors

		#region Properties

		public string Description {
			get { return description; }
			set { description = value; }
		}

		public string Role {
			get { return role; }
			set { role = value; }
		}

		public bool SetEveryoneAccess {
			get { return everyone; }
			set { everyone = value; }
		}

		#endregion // Properties
	}
}
