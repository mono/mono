// 
// System.EnterpriseServices.ApplicationActivationAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly)]
	[ComVisible(false)]
	public sealed class ApplicationActivationAttribute : Attribute, IConfigurationAttribute {

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

		#region Implementation of IConfigurationAttribute

		[MonoTODO]
		bool IConfigurationAttribute.AfterSaveChanges (Hashtable info)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IConfigurationAttribute.Apply (Hashtable cache)
		{
			throw new NotImplementedException ();
		}

		bool IConfigurationAttribute.IsValidTarget (string s)
		{
			return (s == "Application");
		}

		#endregion Implementation of IConfigurationAttribute

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
