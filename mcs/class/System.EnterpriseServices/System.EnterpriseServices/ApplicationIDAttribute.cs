// 
// System.EnterpriseServices.ApplicationIDAttribute.cs
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
	public sealed class ApplicationIDAttribute : Attribute, IConfigurationAttribute {

		#region Fields

		Guid guid;

		#endregion // Fields

		#region Constructors

		public ApplicationIDAttribute (string guid)
		{
			this.guid = new Guid (guid);
		}

		#endregion // Constructors

		#region Implementation of IConfigurationAttribute

		bool IConfigurationAttribute.AfterSaveChanges (Hashtable info)
		{
			return false;
		}

		bool IConfigurationAttribute.Apply (Hashtable cache)
		{
			return false;
		}

		bool IConfigurationAttribute.IsValidTarget (string s)
		{
			return (s == "Application");
		}

		#endregion Implementation of IConfigurationAttribute

		#region Properties

		public Guid Value {	
			get { return guid; }
		}

		#endregion // Properties
	}
}
