// 
// System.IO.IODescriptionAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;

namespace System.IO {
	[AttributeUsage (AttributeTargets.All)]
	public class IODescriptionAttribute : DescriptionAttribute {

		#region Constructors

		public IODescriptionAttribute (string description)
			: base (description)
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override string Description {
			get { return DescriptionValue; }
		}

		#endregion // Methods
	}
}
