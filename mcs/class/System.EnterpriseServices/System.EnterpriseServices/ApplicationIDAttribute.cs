// 
// System.EnterpriseServices.ApplicationIDAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class ApplicationIDAttribute : Attribute {

		#region Fields

		Guid guid;

		#endregion // Fields

		#region Constructors

		public ApplicationIDAttribute (string guid)
		{
			this.guid = new Guid (guid);
		}

		#endregion // Constructors

		#region Properties

		public Guid Value {	
			get { return guid; }
		}

		#endregion // Properties
	}
}
