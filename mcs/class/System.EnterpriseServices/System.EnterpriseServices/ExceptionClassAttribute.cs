// 
// System.EnterpriseServices.ExceptionClassAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ExceptionClassAttribute : Attribute {

		#region Fields

		string name;

		#endregion // Fields

		#region Constructors

		public ExceptionClassAttribute (string name)
		{
			this.name = name;
		}

		#endregion // Constructors

		#region Properties

		public string Value {
			get { return name; }
		}

		#endregion // Properties
	}
}
