// 
// System.EnterpriseServices.ApplicationActivationAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class ApplicationActivationAttribute : Attribute {

		#region Fields

		ActivationOption opt;
		string soapMailbox;
		string soapVRoot;	

		#endregion // Fields

		#region Constructors

		public ApplicationActivationAttribute (ActivationOption opt)
		{
			this.opt = opt;
		}

		#endregion // Constructors

		#region Properties

		public string SoapMailbox {	
			get { return soapMailbox; }
			set { soapMailbox = value; }
		}

		public string SoapVRoot {
			get { return soapVRoot; }
			set { soapVRoot = value; }
		}

		public ActivationOption Value {
			get { return opt; }
		}

		#endregion // Properties
	}
}
