//
// System.DirectoryServices.DSDescriptionAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2004 Andreas Nahr
//
//

using System.ComponentModel;

namespace System.DirectoryServices {

	[AttributeUsage (AttributeTargets.All)]
	public class DSDescriptionAttribute : DescriptionAttribute
	{

		public DSDescriptionAttribute (String description)
			: base (description)
		{
		}

		public override String Description {
			get { return base.Description; }
		}
	}
}

