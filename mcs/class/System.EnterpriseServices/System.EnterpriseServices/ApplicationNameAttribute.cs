// 
// System.EnterpriseServices.ApplicationNameAttribute.cs
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
	public sealed class ApplicationNameAttribute : Attribute, IConfigurationAttribute {

		#region Fields

		string name;

		#endregion // Fields

		#region Constructors

		public ApplicationNameAttribute (string name)
		{
			this.name = name;
		}

		#endregion // Constructors

		#region Implementation of IConfigurationAttribute

		bool IConfigurationAttribute.AfterSaveChanges (Hashtable info)
		{
			return false;
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

		public string Value {	
			get { return name; }
		}

		#endregion // Properties
	}
}
