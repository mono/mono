// 
// System.Data.Odbc/OdbcDescriptionAttribute.cs
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Novell Inc, 2004
//

using System;
using System.ComponentModel;

namespace System.Data {
	[AttributeUsage (AttributeTargets.All)]
	internal sealed class OdbcDescriptionAttribute : DescriptionAttribute
	{
		#region Fields

		string description;

		#endregion // Fields

		#region Constructors

		public OdbcDescriptionAttribute (string description)
			: base (description)
		{
			this.description = description; 
		}

		#endregion // Constructors

		#region Properties

		public override string Description {
			get { return description; }
		}

		#endregion // Properties
	}
}
