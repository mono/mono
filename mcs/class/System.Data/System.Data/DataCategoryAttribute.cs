// 
// System.Data/DataCategoryAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;

namespace System.Data {
	[AttributeUsage (AttributeTargets.All)]
	internal sealed class DataCategoryAttribute : CategoryAttribute
	{
		#region Fields

		string category;

		#endregion // Fields

		#region Constructors

		public DataCategoryAttribute (string category)
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
