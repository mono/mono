//
// System.Web.WebSysDescriptionAttribute.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;

namespace System.Web
{
	[AttributeUsage(AttributeTargets.All)]
	internal class WebSysDescriptionAttribute : DescriptionAttribute
	{
		private bool isReplaced = false;

		public WebSysDescriptionAttribute (string description)
			: base (description)
		{
		}

		public override string Description {
			get {
				if (!isReplaced) {
					isReplaced = true;
					DescriptionValue = Locale.GetText (DescriptionValue);
				}
				return DescriptionValue;
			}
		}
	}
}
