// 
// System.Data.Odbc/OdbcCategoryAttribute.cs
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Novell Inc, 2004
//

using System;
using System.ComponentModel;

namespace System.Data.Odbc {
	[AttributeUsage (AttributeTargets.All)]
	internal sealed class OdbcCategoryAttribute : CategoryAttribute
	{
		#region Fields

		string category;

		#endregion // Fields

		#region Constructors

		public OdbcCategoryAttribute (string category)
		{
			this.category = category; 
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		protected override string GetLocalizedString (string value)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
