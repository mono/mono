// 
// System.EnterpriseServices.ApplicationAccessControlAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class ApplicationAccessControlAttribute : Attribute {

		#region Fields

		AccessChecksLevelOption accessChecksLevel;
		AuthenticationOption authentication;
		ImpersonationLevelOption impersonation;
		bool val;

		#endregion // Fields

		#region Constructors

		public ApplicationAccessControlAttribute ()
		{
			this.val = false;
		}

		public ApplicationAccessControlAttribute (bool val)
		{
			this.val = val;
		}

		#endregion // Constructors

		#region Properties

		public AccessChecksLevelOption AccessChecksLevel {
			get { return accessChecksLevel; }
			set { accessChecksLevel = value; }
		}

		public AuthenticationOption Authentication {
			get { return authentication; }
			set { authentication = value; }
		}

		public ImpersonationLevelOption Impersonation {
			get { return impersonation; }
			set { impersonation = value; }
		}

		public bool Value {
			get { return val; }
			set { val = value; }
		}

		#endregion // Properties
	}
}
