// 
// System.EnterpriseServices.SecurityRoleAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface)]
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
